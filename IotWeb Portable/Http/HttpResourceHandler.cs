using System.IO;
using System.Linq;
using System.Reflection;

namespace IotWeb.Common.Http
{
    public class HttpResourceHandler : IHttpRequestHandler
    {
        // Instance variables
        private Assembly m_assembly;
        private string m_prefix;
        private string m_defaultFile;

        public HttpResourceHandler(Assembly assembly, string prefix, string defaultFile = null)
        {
            m_assembly = assembly;
            m_prefix = string.Format("{0}.{1}", m_assembly.GetName().Name, prefix).Replace(' ', '_');
            m_defaultFile = defaultFile;
        }

        private static string RequestToNamespace(string uri)
        {
            var urlParts = uri.Split('/');
            var fileName = urlParts.Last();
            var location = string.Join(".", urlParts.Take(urlParts.Length - 1));
            var locationNs = location.Replace("@", "_").Replace("-", "_");

            var resourceNs = locationNs + "." + fileName;
            return resourceNs;
        }

        public void HandleRequest(string uri, HttpRequest request, HttpResponse response, HttpContext context)
        {
            if (request.Method != HttpMethod.Get)
                throw new HttpMethodNotAllowedException();
            // Replace '/' with '.' and special characters with _ to generate the resource name
            string resourceName = RequestToNamespace(uri);
            if (resourceName.StartsWith("."))
                resourceName = m_prefix + resourceName;
            else
                resourceName = string.Format("{0}.{1}", m_prefix, resourceName);
            Stream resource = null;
            // If we are not expecting a directory, try and load the stream
            if (!resourceName.EndsWith("."))
                resource = m_assembly.GetManifestResourceStream(resourceName);
            else
                resourceName = resourceName.Substring(0, resourceName.Length - 1);
            // If nothing found try and load the default filename instead
            if ((resource == null) && (m_defaultFile != null))
            {
                // Try the default file
                resourceName = string.Format("{0}.{1}", resourceName, m_defaultFile);
                resource = m_assembly.GetManifestResourceStream(resourceName);
            }
            if (resource == null)
                throw new HttpNotFoundException();
            // Get the mime type and send the file
            response.Headers[HttpHeaders.ContentType] = MimeType.FromExtension(resourceName);
            resource.CopyTo(response.Content);
        }
    }
}
