using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common;
using Splat;

namespace IotWeb.Common.Echo
{
	public class BaseEchoServer : IServer, IEnableLogger
	{
		private ISocketServer m_server;

		public int Port
		{
			get { return m_server.Port; }
		}

		protected BaseEchoServer(ISocketServer server)
		{
			m_server = server;
			m_server.ConnectionRequested = ConnectionRequested;
		}

		public void Start(int port)
		{
			m_server.Start(port);
		}

		public void Stop()
		{
			m_server.Stop();
		}

		private void ConnectionRequested(ISocketServer server, string hostname, Stream input, Stream output)
		{
			this.Log().Debug("Running echo session for {0}", hostname);
			byte[] buffer = new byte[32];
			while (true) 
			{
				int read = input.Read(buffer, 0, buffer.Length);
				if (read == 0)
					break;
				output.Write(buffer, 0, read);
			}
			this.Log().Debug("Connection terminated.");
		}
	}
}
