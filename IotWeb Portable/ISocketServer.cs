using System.IO;

namespace IotWeb.Common
{
	public delegate void ConnectionHandler(ISocketServer sender, string hostname, Stream input, Stream output);

	public interface ISocketServer : IServer
	{
        /// <summary>
        /// Delegate invoked when a new connection is accepted
        /// </summary>
		ConnectionHandler ConnectionRequested { get; set; }
	}
}
