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

		// States for the request parser
		private enum RequestParseState
		{
			StartLine,
			Headers,
			Body
		}

		// Constants
		private const int InputBufferSize = 1024;
		private const byte CR = 0x0d;
		private const byte LF = 0x0a;

		// Instance variables
		private byte[] m_buffer;
		private int m_index;
		private bool m_connected;

		public HttpRequestProcessor()
		{
			m_buffer = new byte[InputBufferSize];
			m_index = 0;
			m_connected = true;
		}

		/// <summary>
		/// Handle the HTTP connection
		/// 
		/// This implementation doesn't support keep alive so each HTTP session
		/// consists of parsing the request, dispatching to a handler and then
		/// sending the response before closing the connection.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		public void ProcessHttpRequest(Stream input, Stream output)
		{
			// Parse the request first
			RequestParseState state = RequestParseState.StartLine;
			string line;
			HttpRequest request;
			HttpResponse response;
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
						if (line.Length==0)
							state++;
						break;
				}
			}
		}

		#region Internal Implementation
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
