using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
