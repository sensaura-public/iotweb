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
        HttpRequest Request { get; }
        HttpResponse Response { get; }
        HttpContext Context { get; }
    }
}
