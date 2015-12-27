namespace IotWeb.Common.Http
{
    /// <summary>
    /// Defines a request handler.
    /// 
    /// Request handler instances are mapped to a URI and, if matched by the
    /// incoming request are invoked to generate the appropriate response.
    /// </summary>
    public interface IHttpRequestHandler
    {
        /// <summary>
        /// Handle a request
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        void HandleRequest(string uri, HttpRequest request, HttpResponse response, HttpContext context);
    }
}
