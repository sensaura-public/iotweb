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

		internal HttpResponse()
			: base()
		{
			ResponseCode = HttpResponseCode.Ok;
			ResponseMessage = null;
		}

		/// <summary>
		/// Write the response to the output stream
		/// </summary>
		/// <param name="output"></param>
		internal void Send(Stream output)
		{

		}
	}
}
