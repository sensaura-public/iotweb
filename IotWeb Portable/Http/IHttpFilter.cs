using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Apply the filter.
        /// 
        /// A filter may modify properties of the request, the response and
        /// the context. This changes may effect how the request is processed
        /// by a handler.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        void ApplyFilter(HttpRequest request, HttpResponse response, HttpContext context);
    }
}
