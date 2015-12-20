using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IotWeb.Common;
using Splat;

namespace IotWeb.Common.Http
{
	class HttpRequestProcessor : IEnableLogger
	{
		// Regular expression for parsing the start line
		private static Regex RequestStartLine = new Regex(@"^([a-zA-z]+)[ ]+([^ ]+)[ ]+[hH][tT][tT][pP]/([0-9]\.[0-9])$");

		// Regular expression for parsing headers
		private static Regex HeaderLine = new Regex(@"^([a-zA-Z][a-zA-Z0-9\-]*):(.*)");

		// States for the request parser
		private enum RequestParseState
		{
			StartLine,
			Headers,
			Body
		}

		// Constants
		private const int MaxRequestBody = 64 * 1024; // TODO: Should be user configurable
		private const int InputBufferSize = 1024;
		private const byte CR = 0x0d;
		private const byte LF = 0x0a;

		// Instance variables
		private byte[] m_buffer;
		private int m_index;
		private bool m_connected;
		private string m_lastHeader;

		public HttpRequestProcessor()
		{
			m_buffer = new byte[InputBufferSize];
			m_index = 0;
			m_connected = true;
			m_lastHeader = null;
		}

		/// <summary>
		/// Handle the HTTP connection
		/// 
		/// This implementation doesn't support keep alive so each HTTP session
		/// consists of parsing the request, dispatching to a handler and then
		/// sending the response before closing the connection.
		/// </summary>
        /// <param name="server"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		public void ProcessHttpRequest(BaseHttpServer server, Stream input, Stream output)
		{
			// Parse the request first
			RequestParseState state = RequestParseState.StartLine;
			string line;
			HttpRequest request = null;
			HttpException parseError = null;
			try
			{
				while (m_connected && (state != RequestParseState.Body))
				{
					// Keep trying to read a line
					if (!ReadLine(input, out line))
						continue;
					switch (state)
					{
						case RequestParseState.StartLine:
							request = ParseRequestLine(line);
							if (request == null)
								return; // Just let the connection close
							state++;
							break;
						case RequestParseState.Headers:
							if (line.Length == 0)
								state++;
							else
								ParseHeaderLine(request, line);
							break;
					}
				}
			}
			catch (HttpException ex)
			{
				parseError = ex;
			}
			catch (Exception ex)
			{
				parseError = new HttpInternalServerErrorException("Error parsing request.");
			}
			// Read any associated body component
			if ((parseError == null)&&request.Headers.ContainsKey(HttpHeaders.ContentType))
			{
				request.ContentType = request.Headers[HttpHeaders.ContentType];
				try 
				{
					if (!request.Headers.ContainsKey(HttpHeaders.ContentLength))
						throw new HttpLengthRequiredException();
					int length;
					if (!int.TryParse(request.Headers[HttpHeaders.ContentLength], out length))
						throw new HttpLengthRequiredException();
					request.ContentLength = length;
					if (length > MaxRequestBody)
						throw new HttpRequestEntityTooLargeException();
					// Read the data in
					MemoryStream content = new MemoryStream();
					while (m_connected && (content.Length != length)) 
					{
						ReadData(input);
						content.Write(m_buffer, 0, m_index);
						ExtractBytes(m_index);
					}
					// Did the connection drop while reading?
					if (!m_connected)
						return;
					// Reset the stream location and attach it to the request
					content.Seek(0, SeekOrigin.Begin);
					request.Content = content;
				}
				catch (HttpException ex)
				{
					parseError = ex;
				}
				catch (Exception ex)
				{
					parseError = new HttpInternalServerErrorException();
				}
			}
            // We have at least a partial request, create the matching response
            HttpContext context = new HttpContext();
            HttpResponse response = new HttpResponse();
			if (parseError == null)
            {
                // TODO: Process the cookies
                // Apply filters
                try
                {
                    server.ApplyFilters(request, response, context);
                }
                catch (HttpException ex)
                {
                    parseError = ex;
                }
                catch (Exception ex)
                {
                    parseError = new HttpInternalServerErrorException();
                }
            }
            // TODO: Check for WebSocket upgrade
            // Dispatch to the handler
            try
            {
                string partialUri;
                IHttpRequestHandler handler = server.GetHandlerForUri(request.URI, out partialUri);
                if (handler == null)
                    throw new HttpNotFoundException();
                handler.HandleRequest(partialUri, request, response, context);
            }
            catch (HttpException ex)
            {
                parseError = ex;
            }
            catch (Exception ex)
            {
                parseError = new HttpInternalServerErrorException();
            }
            if (parseError != null)
            {
                // Can't continue, send a response back with the error information
                response.ResponseCode = parseError.ResponseCode;
				response.ResponseMessage = parseError.Message;
				response.Send(output);
			}
        }

        #region Internal Implementation
        private void ParseHeaderLine(HttpRequest request, string line)
		{
			if (line.StartsWith(" "))
			{
				// Continuation
				if (m_lastHeader == null)
					throw new HttpBadRequestException("Invalid header format.");
				request.Headers[m_lastHeader] = request.Headers[m_lastHeader] + line;
			}
			else
			{
				Match match = HeaderLine.Match(line);
				if (match.Groups.Count != 3)
					throw new HttpBadRequestException("Cannot parse header.");
				m_lastHeader = match.Groups[1].Value.Trim();
				request.Headers[m_lastHeader] = match.Groups[2].Value.Trim();
			}
		}

		/// <summary>
		/// Parse the request start line
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private HttpRequest ParseRequestLine(string line)
		{
			Match match;
			lock (RequestStartLine)
				match = RequestStartLine.Match(line);
			if (match.Groups.Count != 4)
				return null;
			// Get the method used
			HttpMethod method;
			if (!Enum.TryParse<HttpMethod>(match.Groups[1].Value, true, out method))
				return null;
			HttpRequest request = new HttpRequest(method, match.Groups[2].Value);
			// TODO: Should really check the HTTP version here as well
			return request;
		}

		/// <summary>
		/// Extract bytes from the input buffer (with an optional copy)
		/// </summary>
		/// <param name="count"></param>
		/// <param name="copy"></param>
		/// <returns></returns>
		private int ExtractBytes(int count, byte[] copy = null)
		{
			// Trim the number of bytes to that available
			count = (count > m_index) ? m_index : count;
			// Make a copy if requested
			if (copy != null)
				Array.Copy(m_buffer, copy, count);
			// Shuffle everything down
			Array.Copy(m_buffer, count, m_buffer, 0, m_index - count);
			m_index -= count;
			return count;
		}

		/// <summary>
		/// Read data from the stream into the buffer.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private void ReadData(Stream input)
		{
			bool closed = false;
			try
			{
				int read = input.Read(m_buffer, m_index, m_buffer.Length - m_index);
				m_index += read;
				if (read == 0)
					m_connected = false;
			}
			catch (Exception)
			{
				// Any error causes the connection to close
				m_connected = false;
			}
		}

		/// <summary>
		/// Read a line (terminated by CR/LF) from the input stream
		/// </summary>
		/// <param name="input"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private bool ReadLine(Stream input, out string line)
		{
			line = null;
			// Make sure we have some data in the buffer
			ReadData(input);
			if (!m_connected)
				return false;
			// Look for CR/LF pair
			for (int i = 0; i < (m_index - 1); i++)
			{
				if ((m_buffer[i] == CR) && (m_buffer[i + 1] == LF))
				{
					// Extract the string (without the CR/LF)
					line = Encoding.UTF8.GetString(m_buffer, 0, i);
					ExtractBytes(i + 2);
					return true;
				}
			}
			// No line yet
			return false;
		}
		#endregion
	}
}
