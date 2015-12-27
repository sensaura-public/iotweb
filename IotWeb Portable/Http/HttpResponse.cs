using System;
using System.IO;
using System.Net;
using System.Text;

namespace IotWeb.Common.Http
{
	/// <summary>
	/// Represents a HTTP response
	/// </summary>
	public class HttpResponse : HttpPDU
	{
		public HttpResponseCode ResponseCode { get; set; }

		public string ResponseMessage { get; set; }

		internal HttpResponse()
			: base()
		{
			ResponseCode = HttpResponseCode.Ok;
			ResponseMessage = null;
            Content = new MemoryStream();
		}

		/// <summary>
		/// Write the response to the output stream
		/// </summary>
		/// <param name="output"></param>
		internal void Send(Stream output)
		{
            // Write the response start line
            WriteLine(output, String.Format("HTTP/1.0 {0} {1}", ResponseCode.ResponseCode(), ResponseCode.ResponseMessage(ResponseMessage)));
            // Set content length accordingly
            Headers[HttpHeaders.ContentLength] = Content.Position.ToString();
            // Write the headers
            foreach (string header in Headers.Keys)
                WriteLine(output, string.Format("{0}: {1}", header, Headers[header]));
            // Write the cookies
			if (Cookies.Count > 0) 
			{
				StringBuilder sb = new StringBuilder();
				foreach (Cookie cookie in Cookies)
				{
					sb.Append(HttpHeaders.SetCookie + ": ");
					// Add the value
					sb.Append(String.Format("{0}={1}", cookie.Name, cookie.Value));
					// Add the path
					if ((cookie.Path != null) && (cookie.Path.Length > 0))
						sb.Append(string.Format("; Path={0}", cookie.Path));
					// Add the domain
					if ((cookie.Domain != null) && (cookie.Domain.Length > 0))
						sb.Append(string.Format("; Domain={0}", cookie.Domain));
					// Set the expiry
					if (cookie.Expires != DateTime.MinValue) 
					{
						DateTime utc = cookie.Expires.ToUniversalTime();
						sb.Append("; Expires=");
						sb.Append(utc.ToString(@"ddd, dd-MMM-yyyy HH:mm:ss G\MT"));
					}
					// Set the security
					if (cookie.Secure)
						sb.Append("; Secure");
					if (cookie.HttpOnly)
						sb.Append("; HttpOnly");
					// Write the header
					WriteLine(output, sb.ToString());
					sb.Clear();
				}
			}
            // Write the body
            WriteLine(output, "");
            long bytesToWrite = Content.Position;
            Content.SetLength(bytesToWrite);
            Content.Seek(0, SeekOrigin.Begin);
            Content.CopyTo(output);
		}

        /// <summary>
        /// Write a single line in UTF8 terminated by a CR/LF pair
        /// </summary>
        /// <param name="output"></param>
        /// <param name="line"></param>
        private void WriteLine(Stream output, string line)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(line + "\r\n");
            output.Write(bytes, 0, bytes.Length);
        }
	}
}
