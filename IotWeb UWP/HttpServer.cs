using IotWeb.Common.Http;
using IotWeb.Common.Util;
using IotWeb.Server.Helper;

namespace IotWeb.Server
{
    public class HttpServer : BaseHttpServer
    {
        public HttpServer(int port, SessionStorageConfiguration sessionStorageConfiguration)
            : base(new SocketServer(port), new HybridSessionStorageHandler(sessionStorageConfiguration))
        {
            // No configuration required
        }
    }
}
