namespace IotWeb.Common.Http
{
	/// <summary>
	/// HTTP response codes
	/// 
	/// Only a subset are included here. Taken from the HTTP/1.0 spec:
	/// http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html
	/// </summary>
	public enum HttpResponseCode
	{
		SwitchingProtocols,
		Ok,
		MovedPermanently,
		SeeOther,
		BadRequest,
		Unauthorized,
		Forbidden,
		NotFound,
		MethodNotAllowed,
		LengthRequired,
		RequestEntityTooLarge,
		InternalServerError,
		NotImplemented,
		VersionNotSupported
	}

	/// <summary>
	/// Extension methods for HttpResponseCode enums
	/// </summary>
	public static class HttpResponseCodeExtensions
	{
		/// <summary>
		/// Map enums to response code values
		/// </summary>
		private static int[] ResponseCodes = {
			101, // SwitchingProtocols
			200, // Ok
			301, // MovedPermanently
			303, // SeeOther
			400, // BadRequest
			401, // Unauthorized
			403, // Forbidden
			404, // NotFound
			405, // MethodNotAllowed
			411, // LengthRequired
			413, // RequestEntityTooLarge
			500, // InternalServerError
			501, // NotImplemented
			505, // VersionNotSupported
			};

		/// <summary>
		/// Map enums to response messages
		/// </summary>
		private static string[] ResponseMessages = {
			"Switching Protocols",
			"OK",
			"Moved Permanently",
			"See Other",
			"Bad Request",
			"Unauthorized",
			"Forbidden",
			"Not Found",
			"Method Not Allowed",
			"Length Required",
			"Request Entity Too Large",
			"Internal Server Error",
			"Not Implemented",
			"Version Not Supported",
			};

		/// <summary>
		/// Get the actual response code assocatiated with the ResponseCode enum
		/// </summary>
		/// <param name="responseCode"></param>
		/// <returns></returns>
		public static int ResponseCode(this HttpResponseCode responseCode)
		{
			return ResponseCodes[(int)responseCode];
		}

		/// <summary>
		/// Get the message associated with the ResponseCode enum
		/// </summary>
		/// <param name="responseCode"></param>
		/// <returns></returns>
		public static string ResponseMessage(this HttpResponseCode responseCode, string alternate = null)
		{
			if (alternate != null)
				return alternate;
			return ResponseMessages[(int)responseCode];
		}
	}
}
