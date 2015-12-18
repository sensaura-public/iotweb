using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
	public class HttpHeaders : IDictionary<string, string>
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

		// The actual headers
		private IDictionary<string, string> m_headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Internal constructor
		/// </summary>
		internal HttpHeaders()
			: base()
		{
			// Do nothing
		}

		#region Implementation of IDictionary
		public bool IsReadOnly
		{
			get { return false; }
		}

		public ICollection<string> Keys
		{
			get { return m_headers.Keys; }
		}

		public ICollection<string> Values
		{
			get { return m_headers.Values; }
		}

		public int Count
		{
			get { return m_headers.Count; }
		}

		public string this[string key]
		{
			get
			{
				return m_headers[key];
			}
			set
			{
				m_headers[key] = value;
			}
		}

		public void Add(string key, string value)
		{
			m_headers.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return m_headers.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return m_headers.Remove(key);
		}

		public bool TryGetValue(string key, out string value)
		{
			return m_headers.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<string, string> item)
		{
			m_headers.Add(item);
		}

		public void Clear()
		{
			m_headers.Clear();
		}

		public bool Contains(KeyValuePair<string, string> item)
		{
			return m_headers.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			m_headers.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, string> item)
		{
			return m_headers.Remove(item);
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return m_headers.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable)m_headers).GetEnumerator();
		}
		#endregion
	}
}
