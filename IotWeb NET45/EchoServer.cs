using IotWeb.Common.Echo;

namespace IotWeb.Server
{
	public class EchoServer : BaseEchoServer
	{
		public EchoServer() : base(new SocketServer())
		{
			// No configuration required
		}
	}
}
