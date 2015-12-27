using System.IO;
using System.Net;

namespace IotWeb.Common.Http
{
	/// <summary>
	/// Base class for a HTTP protocol data unit (PDU).
	/// 
	/// This provides implementation details that are common to a request
	/// and a response.
	/// </summary>
	public class HttpPDU
	{
		/// <summary>
		/// The cookies associated with the transaction.
		/// </summary>
		public readonly CookieCollection Cookies = new CookieCollection();

		/// <summary>
		/// The headers associated with the transaction.
		/// </summary>
		public readonly HttpHeaders Headers = new HttpHeaders();

		/// <summary>
		/// The content type of the data associated with the operation
		/// 
		/// This property may be null (or the empty string) if no content is
		/// available.
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// The length (in bytes) of the content associated with the operation.
		/// </summary>
		public int ContentLength { get; set; }

		/// <summary>
		/// A stream to represent the content associated with the transaction
		/// </summary>
		public Stream Content { get; internal set; }

		/// <summary>
		/// Only internal classes can extend this.
		/// </summary>
		internal HttpPDU()
		{
			ContentType = null;
			ContentLength = 0;
		}
	}
}
