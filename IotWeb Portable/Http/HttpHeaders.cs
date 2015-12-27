namespace IotWeb.Common.Http
{
	public class HttpHeaders : CaseInsensitiveDictionary<string>
	{
		// Common header fields
		public const string Accept = "Accept";
		public const string AcceptCharset = "Accept-Charset";
		public const string AcceptEncoding = "Accept-Encoding";
		public const string AcceptLanguage = "Accept-Language";
		public const string AcceptDatetime = "Accept-Datetime";
		public const string Authorization = "Authorization";
		public const string Connection = "Connection";
		public const string Cookie = "Cookie";
		public const string ContentLength = "Content-Length";
		public const string ContentMD5 = "Content-MD5";
		public const string ContentType = "Content-Type";
		public const string Host = "Host";
		public const string Pragma = "Pragma";
		public const string Referer = "Referer";
		public const string UserAgent = "User-Agent";
		public const string Upgrade	= "Upgrade";
		public const string ContentLanguage = "Content-Language";
		public const string ContentEncoding = "Content-Encoding";
		public const string Refresh = "Refresh";
		public const string SetCookie = "Set-Cookie";
		public const string Server = "Server";
		public const string WWWAuthenticate = "WWW-Authenticate";
	}
}
