using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IotWeb.Common;

namespace IotWeb.Server
{
	public class SocketServer : ISocketServer
	{
		// Constants
		private const int BackLog = 5; // Maximum pending requests

		// Instance variables
		private bool m_running;
		private ConnectionHandler m_handler;
        private Socket m_listener;

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
            ThreadPool.QueueUserWorkItem((arg) =>
            {
                m_listener = new Socket(SocketType.Stream, ProtocolType.IP);
                m_listener.Bind(new IPEndPoint(IPAddress.Any, port));
                m_listener.Blocking = true;
                m_listener.ReceiveTimeout = 100;
                m_listener.Listen(BackLog);
                // Wait for incoming connections
                while (true)
                {
                    lock (this)
                    {
                        if (!m_running)
                            return;
                    }
                    try
                    {
                        Socket client;
                        try
                        {
                            client = m_listener.Accept();
                        }
                        catch (TimeoutException)
                        {
                            // Allow recheck of running status
                            continue;
                        }
                        if (m_handler != null)
                        {
                            string hostname = "0.0.0.0";
                            IPEndPoint endpoint = client.RemoteEndPoint as IPEndPoint;
                            if (endpoint != null)
                                hostname = endpoint.Address.ToString();
                            ThreadPool.QueueUserWorkItem((e) =>
                            {
                                try
                                {
                                    if (m_handler != null)
                                    {
                                        client.ReceiveTimeout = 0;
                                        m_handler(
                                            this,
                                            hostname,
                                            new NetworkStream(client, FileAccess.Read, false),
                                            new NetworkStream(client, FileAccess.Write, false)
                                            );
                                    }
                                }
                                catch (Exception)
                                {
                                    // Quietly consume the exception
                                }
                            // Finally, we can close the socket
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // Quietly consume the exception
                    }
                }
            });
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
