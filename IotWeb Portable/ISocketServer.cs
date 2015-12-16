using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common
{
	public delegate void ConnectionHandler(ISocketServer sender, string hostname, Stream input, Stream output);

	public interface ISocketServer
	{
		ConnectionHandler ConnectionRequested { get; set; }

		int Port { get; }

		void Start(int port);

		void Stop();
	}
}
