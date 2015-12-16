using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common
{
	public delegate void ConnectionHandler(ISocketServer sender, string hostname, Stream input, Stream output);

	public interface ISocketServer : IServer
	{
		ConnectionHandler ConnectionRequested { get; set; }
	}
}
