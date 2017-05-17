using IotWeb.Common.Http;
using IotWeb.Common.Util;
using IotWeb.Server.Helper;

namespace IotWeb.Server
{
	public class HttpServer : BaseHttpServer
	{
        public HttpServer(int port)
           : base(new SocketServer(port), new HybridSessionStorageHandler(new SessionConfiguration()))
        {
            // No configuration required
        }

        public HttpServer(int port, SessionConfiguration sessionConfiguration)
            : base(new SocketServer(port), new HybridSessionStorageHandler(sessionConfiguration))
        {
			// No configuration required
		}
	}
}
