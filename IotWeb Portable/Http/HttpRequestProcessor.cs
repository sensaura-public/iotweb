using System;
using System.IO;
using System.Net;
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

		// Cookie separators
		private static char[] CookieSeparator = new char[] { ';' };
		private static char[] CookieValueSeparator = new char[] { '=' };

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
        private BaseHttpServer m_server;

        /// <summary>
        /// Default constructor
        /// </summary>
		public HttpRequestProcessor(BaseHttpServer server)
		{
			m_buffer = new byte[InputBufferSize];
			m_index = 0;
			m_connected = true;
			m_lastHeader = null;
            m_server = server;
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
        public void ProcessHttpRequest(Stream input, Stream output)
		{
            this.Log().Debug("** Processing new request");
            // Set up state
            HttpRequest request = null;
            HttpResponse response = null;
            HttpException parseError = null;
            // Process the request
            try
            {
                request = ParseRequest(input);
                if ((request == null) || !m_connected)
                    return; // Nothing we can do, just drop the connection
                // Do we have any content in the body ?
                if (request.Headers.ContainsKey(HttpHeaders.ContentType))
                {
                    this.Log().Debug("Reading request content.");
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
                // Process the cookies
				this.Log().Debug("Processing cookies");
				if (request.Headers.ContainsKey(HttpHeaders.Cookie))
				{
					string[] cookies = request.Headers[HttpHeaders.Cookie].Split(CookieSeparator);
					foreach (string cookie in cookies)
					{
						string[] parts = cookie.Split(CookieValueSeparator);
						Cookie c = new Cookie();
						c.Name = parts[0];
						if (parts.Length > 1)
							c.Value = parts[1];
						this.Log().Debug("Adding cookie '{0}'", c);
						request.Cookies.Add(c);
					}
				}
				// We have at least a partial request, create the matching response
				HttpContext context = new HttpContext();
				response = new HttpResponse();
				// Apply filters
                this.Log().Debug("Applying middleware filters");
                m_server.ApplyFilters(request, response, context);
                // TODO: Check for WebSocket upgrade
                // Dispatch to the handler
                string partialUri;
                IHttpRequestHandler handler = m_server.GetHandlerForUri(request.URI, out partialUri);
                if (handler == null)
                    throw new HttpNotFoundException();
                this.Log().Debug("Dispatching to handler.");
                handler.HandleRequest(partialUri, request, response, context);
            }
            catch (HttpException ex)
            {
                parseError = ex;
            }
            catch (Exception ex)
            {
                this.Log().Debug("Exception while processing request - {0}", ex.Message);
                parseError = new HttpInternalServerErrorException();
            }
            // Do we need to send back an error response ?
            if (parseError != null)
            {
                // TODO: Clear any content that might already be added
                response.ResponseCode = parseError.ResponseCode;
                response.ResponseMessage = parseError.Message;
            }
            // Write the response
            this.Log().Debug("Sending back response");
            response.Send(output);
            output.Flush();
        }

        #region Internal Implementation
        /// <summary>
        /// Parse the request and headers.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private HttpRequest ParseRequest(Stream input)
        {
            // Parse the request first
            RequestParseState state = RequestParseState.StartLine;
            string line;
            HttpRequest request = null;
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
                            this.Log().Debug("Read request start line - '{0}'", line);
                            request = ParseRequestLine(line);
                            if (request == null)
                                return null; // Just let the connection close
                            state++;
                            break;
                        case RequestParseState.Headers:
                            this.Log().Debug("Read header line - '{0}'", line);
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
                throw ex;
            }
            catch (Exception ex)
            {
                this.Log().Debug("Error while parsing request - {0}", ex.Message);
                throw new HttpInternalServerErrorException("Error parsing request.");
            }
            // All done
            return request;
        }

        /// <summary>
        /// Parse a single header line
        /// </summary>
        /// <param name="request"></param>
        /// <param name="line"></param>
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
			try
			{
				int read = input.Read(m_buffer, m_index, m_buffer.Length - m_index);
				m_index += read;
				if (read == 0)
					m_connected = false;
			}
			catch (Exception ex)
			{
                // Any error causes the connection to close
                this.Log().Debug("Error while reading - {0}", ex.ToString());
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
			// Look for CR/LF pair
			for (int i = 0; i < (m_index - 1); i++)
			{
				if ((m_buffer[i] == CR) && (m_buffer[i + 1] == LF))
				{
					// Extract the string (without the CR/LF)
					line = Encoding.UTF8.GetString(m_buffer, 0, i);
					ExtractBytes(i + 2);
                    this.Log().Debug("Read input line '{0}'", line);
					return true;
				}
			}
            // No line yet, read more data
            ReadData(input);
            return false;
		}
		#endregion
	}
}
