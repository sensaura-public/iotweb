namespace IotWeb.Common.Http
{
    /// <summary>
    /// Represents a filter.
    /// 
    /// Filters are used as a simple middleware implementation mechanism, they
    /// are applied after the request has been parsed and the initial response
    /// created but before the request is passed to a handler.
    /// </summary>
    public interface IHttpFilter
    {
        /// <summary>
        /// Invoked before the request has been processed.
        /// 
		/// This method is called when the request has been parsed but before it has
		/// been dispatched to a handler. The filter may modify the request, response
		/// and context information as well as prohibit the request from being handled
		/// at all.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
		/// <returns>
		/// True if the request should be processed in the normal manner or false if
		/// it should not be dispatched to a handler.
		/// </returns>
        bool Before(HttpRequest request, HttpResponse response, HttpContext context);

		/// <summary>
		/// Invoked after the request has been processed.
		/// 
		/// This method is called when the request has been processed by a handler
		/// (or it has been determined that no handler is available for the request).
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <param name="context"></param>
		void After(HttpRequest request, HttpResponse response, HttpContext context);
    }
}
