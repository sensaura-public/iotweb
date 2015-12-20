using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
	/// <summary>
	/// Represents a HTTP response
	/// </summary>
	public class HttpResponse : HttpPDU
	{
		public HttpResponseCode ResponseCode { get; set; }

		public string ResponseMessage { get; set; }

        public Stream Content { get; private set; }

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
            // TODO: Write the cookies
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
