using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IotWeb.Common;
using Splat;

namespace IotWeb.Server
{
	public class SocketServer : ISocketServer, IEnableLogger
	{
		// Constants
		private const int BackLog = 5; // Maximum pending requests

		// Instance variables
		private bool m_running;
		private ConnectionHandler m_handler;

		public int Port { get; private set; }

		public ConnectionHandler ConnectionRequested
		{
			get
			{
				return m_handler;
			}

			set
			{
				lock (this)
				{
					if (m_running)
						throw new InvalidOperationException("Cannot change handler while server is running.");
					m_handler = value;
				}
			}
		}

		public void Start(int port)
		{
			// Make sure we are not already running
			lock (this)
			{
				if (m_running)
					throw new InvalidOperationException("Socket server is already running.");
				m_running = true;
			}
			Port = port;
			// Set up the listener and bind
			Socket listener = new Socket(SocketType.Stream, ProtocolType.IP);
			listener.Bind(new IPEndPoint(IPAddress.Any, port));
			listener.Blocking = true;
			listener.Listen(BackLog);
			// Wait for incoming connections
			while (m_running)
			{
				try
				{
					Socket client = listener.Accept();
					if (m_handler != null)
					{
						string hostname = "0.0.0.0";
						IPEndPoint endpoint = client.RemoteEndPoint as IPEndPoint;
						if (endpoint != null) 
							hostname = endpoint.Address.ToString();
						ThreadPool.QueueUserWorkItem((arg) =>
						{
							try
							{
								m_handler(
									this, 
									hostname,
									new NetworkStream(client, FileAccess.Read, false),
									new NetworkStream(client, FileAccess.Write, false)
									);
							}
							catch (Exception ex)
							{
								this.Log().Debug("Connection handler failed unexpectedly - {0}", ex.Message);
							}
							// Finally, we can close the socket
							client.Shutdown(SocketShutdown.Both);
							client.Close();
						});
					}
				}
				catch (Exception ex)
				{
					this.Log().Debug("Unexpected error while accepting connection request - {0}", ex.Message);
				}
			}
		}

		public void Stop()
		{
			lock (this)
			{
				m_running = false;
			}
		}

	}
}
