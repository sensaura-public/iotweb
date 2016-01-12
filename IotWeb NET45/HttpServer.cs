using IotWeb.Common.Http;

namespace IotWeb.Server
{
	public class HttpServer : BaseHttpServer
	{
		public HttpServer(int port)
			: base(new SocketServer(port))
		{
			// No configuration required
		}
	}
}
