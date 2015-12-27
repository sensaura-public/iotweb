using IotWeb.Common.Http;

namespace IotWeb.Server
{
    public class HttpServer : BaseHttpServer
    {
        public HttpServer()
            : base(new SocketServer())
        {
            // No configuration required
        }
    }
}
