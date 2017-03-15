using System;
using System.Threading;
using IotWeb.Server;
using IotWeb.Common.Util;
using IotWeb.Common.Http;

namespace WebHost.Desktop
{
	/// <summary>
	/// Simple 'echo' web socket server
	/// </summary>
	class WebSocketHandler : IWebSocketRequestHandler
	{

		public bool WillAcceptRequest(string uri, string protocol)
		{
			return (uri.Length == 0) && (protocol == "echo");
		}

		public void Connected(WebSocket socket)
		{
			socket.DataReceived += OnDataReceived;
		}

		void OnDataReceived(WebSocket socket, string frame)
		{
			socket.Send(frame);
		}
	}

	class Program
	{
        static void Main(string[] args)
		{
            // Set up and run the server
			HttpServer server = new HttpServer(8000, new SessionStorageConfiguration());
            server.AddHttpRequestHandler(
                "/",
                new HttpResourceHandler(
                    Utilities.GetContainingAssembly(typeof(Program)),
                    "Resources.Site",
                    "index.html"
                    )
                );
			server.AddWebSocketRequestHandler(
				"/sockets/",
				new WebSocketHandler()
				);
			server.Start();
            Console.WriteLine("Server running - press any key to stop.");
            while (!Console.KeyAvailable)
                Thread.Sleep(100);
            Console.ReadKey();
		}
	}
}
