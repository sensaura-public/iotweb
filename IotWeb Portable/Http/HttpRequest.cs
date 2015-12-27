using System.Net;
using System.Collections.Generic;

namespace IotWeb.Common.Http
{
	/// <summary>
	/// Represents a HTTP request
	/// </summary>
	public class HttpRequest : HttpPDU
	{
		private static char[] QueryStringSeparator = new char[] { '?' };
		private static char[] QueryFieldSeparator = new char[] { '&', ';' };
		private static char[] QueryValueSeparator = new char[] { '=' };

		/// <summary>
		/// The method of the request
		/// </summary>
		public HttpMethod Method { get; private set; }

		/// <summary>
		/// The URI associated with the request
		/// </summary>
		public string URI { get; private set; }

		/// <summary>
		/// The query string component of the request URL
		/// </summary>
		public string QueryString { get; private set; }

		/// <summary>
		/// Constructor with a method
		/// </summary>
		/// <param name="method"></param>
		/// <param name="url"></param>
		internal HttpRequest(HttpMethod method, string url)
			: base()
		{
			Method = method;
			// Split the URL into the URI and the query string
			string[] parts = url.Split(QueryStringSeparator);
			URI = WebUtility.UrlDecode(parts[0]);
			if (parts.Length > 1)
				QueryString = parts[1];
			else
				QueryString = "";
		}

		/// <summary>
		/// Helper method to parse the query string into name/value components
		/// 
		/// This allows for multiple values for a single named field.
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, IList<string>> ParseQueryString()
		{
			Dictionary<string, IList<string>> results = new Dictionary<string,IList<string>>();
			string[] parts = QueryString.Split(QueryFieldSeparator);
			foreach (string field in parts)
			{
				string[] pair = field.Split(QueryValueSeparator);
				string name, value = "";
				if (pair.Length == 1)
				{
					// Just a name, assume an empty value
					name = WebUtility.UrlDecode(pair[0]);
				}
				else if (pair.Length == 2)
				{
					// Name and a value
					name = WebUtility.UrlDecode(pair[0]);
					value = WebUtility.UrlDecode(pair[1]);
				}
				else
					continue; // Just quietly ignore it
				// Add the field to the results
				if (!results.ContainsKey(name))
					results[name] = new List<string>();
				results[name].Add(value);
			}
			return results;
		}
	}
}
