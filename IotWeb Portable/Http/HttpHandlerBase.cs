using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
    public abstract class HttpHandlerBase : IHttpRequestHandler
    {
        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
        
        internal void InitializeHttpHandlerBase(HttpRequest request, HttpResponse response, HttpContext context)
        {
            if (request == null)
                throw new HttpRequestException("Request object can not be null.");

            if (response == null)
                throw new ArgumentException("Response object can not be null.");

            if (context == null)
                throw new ArgumentException("Context object can not be null.");

            Request = request;
            Response = response;
            Context = context;
        }

        /// <summary>
        /// Handle a request
        /// </summary>
        /// <param name="uri"></param>
        public abstract void HandleRequest(string uri);
    }
}
