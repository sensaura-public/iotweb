using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common;
using Splat;

namespace IotWeb.Common.Http
{
	public class BaseHttpServer : IServer, IEnableLogger
	{
		// Instance variables
		private ISocketServer m_server;

		public int Port
		{
			get { return m_server.Port; }
		}

		protected BaseHttpServer(ISocketServer server)
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

		/// <summary>
		/// Handle the HTTP connection
		/// 
		/// This implementation doesn't support keep alive so each HTTP session
		/// consists of parsing the request, dispatching to a handler and then
		/// sending the response before closing the connection.
		/// </summary>
		/// <param name="server"></param>
		/// <param name="hostname"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		private void ConnectionRequested(ISocketServer server, string hostname, Stream input, Stream output)
		{
			HttpRequestProcessor processor = new HttpRequestProcessor();
			processor.ProcessHttpRequest(input, output);
		}

	}
}
