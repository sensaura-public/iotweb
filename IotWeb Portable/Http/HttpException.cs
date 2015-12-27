using System;

namespace IotWeb.Common.Http
{
	public class HttpException : Exception
	{
		/// <summary>
		/// The response code associated with the request
		/// </summary>
		public HttpResponseCode ResponseCode { get; private set; }

		/// <summary>
		/// Constructor with response code and optional message
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="message"></param>
		public HttpException(HttpResponseCode responseCode, string message = null) : base(responseCode.ResponseMessage(message))
		{
			ResponseCode = responseCode;
		}
	}


	/// <summary>
	/// SwitchingProtocols exception
	/// </summary>
	public class HttpSwitchingProtocolsException : HttpException
	{
		public HttpSwitchingProtocolsException(string message = null)
			: base(HttpResponseCode.SwitchingProtocols, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// Ok exception
	/// </summary>
	public class HttpOkException : HttpException
	{
		public HttpOkException(string message = null)
			: base(HttpResponseCode.Ok, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// MovedPermanently exception
	/// </summary>
	public class HttpMovedPermanentlyException : HttpException
	{
		public HttpMovedPermanentlyException(string message = null)
			: base(HttpResponseCode.MovedPermanently, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// SeeOther exception
	/// </summary>
	public class HttpSeeOtherException : HttpException
	{
		public HttpSeeOtherException(string message = null)
			: base(HttpResponseCode.SeeOther, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// BadRequest exception
	/// </summary>
	public class HttpBadRequestException : HttpException
	{
		public HttpBadRequestException(string message = null)
			: base(HttpResponseCode.BadRequest, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// Unauthorized exception
	/// </summary>
	public class HttpUnauthorizedException : HttpException
	{
		public HttpUnauthorizedException(string message = null)
			: base(HttpResponseCode.Unauthorized, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// Forbidden exception
	/// </summary>
	public class HttpForbiddenException : HttpException
	{
		public HttpForbiddenException(string message = null)
			: base(HttpResponseCode.Forbidden, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// NotFound exception
	/// </summary>
	public class HttpNotFoundException : HttpException
	{
		public HttpNotFoundException(string message = null)
			: base(HttpResponseCode.NotFound, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// MethodNotAllowed exception
	/// </summary>
	public class HttpMethodNotAllowedException : HttpException
	{
		public HttpMethodNotAllowedException(string message = null)
			: base(HttpResponseCode.MethodNotAllowed, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// LengthRequired exception
	/// </summary>
	public class HttpLengthRequiredException : HttpException
	{
		public HttpLengthRequiredException(string message = null)
			: base(HttpResponseCode.LengthRequired, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// RequestEntityTooLarge exception
	/// </summary>
	public class HttpRequestEntityTooLargeException : HttpException
	{
		public HttpRequestEntityTooLargeException(string message = null)
			: base(HttpResponseCode.RequestEntityTooLarge, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// InternalServerError exception
	/// </summary>
	public class HttpInternalServerErrorException : HttpException
	{
		public HttpInternalServerErrorException(string message = null)
			: base(HttpResponseCode.InternalServerError, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// NotImplemented exception
	/// </summary>
	public class HttpNotImplementedException : HttpException
	{
		public HttpNotImplementedException(string message = null)
			: base(HttpResponseCode.NotImplemented, message)
		{
			// Nothing to do
		}
	}


	/// <summary>
	/// VersionNotSupported exception
	/// </summary>
	public class HttpVersionNotSupportedException : HttpException
	{
		public HttpVersionNotSupportedException(string message = null)
			: base(HttpResponseCode.VersionNotSupported, message)
		{
			// Nothing to do
		}
	}

}
