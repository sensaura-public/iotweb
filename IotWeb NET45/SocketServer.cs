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
					if (Running)
						throw new InvalidOperationException("Cannot change handler while server is running.");
					m_handler = value;
				}
			}
		}

		public bool Running { get; private set; }

		public event ServerStoppedHandler ServerStopped;

		/// <summary>
		/// Constructor with a port to listen on
		/// </summary>
		/// <param name="port"></param>
		public SocketServer(int port)
		{
			Port = port;
		}

		public void Start()
		{
			// Make sure we are not already running
			lock (this)
			{
				if (Running)
					throw new InvalidOperationException("Socket server is already running.");
				Running = true;
			}
            // Set up the listener and bind
            ThreadPool.QueueUserWorkItem((arg) =>
            {
                m_listener = new Socket(SocketType.Stream, ProtocolType.IP);
                m_listener.Bind(new IPEndPoint(IPAddress.Any, Port));
                m_listener.Blocking = true;
                m_listener.ReceiveTimeout = 100;
                m_listener.Listen(BackLog);
                // Wait for incoming connections
                while (true)
                {
                    lock (this)
                    {
                        if (!Running)
                            break;
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
				// Fire the stopped events
				lock (this)
				{
					ServerStoppedHandler handler = ServerStopped;
					if (handler != null)
						handler(this);
				}
            });
		}

		public void Stop()
		{
			lock (this)
			{
				if (!Running)
					return;
				// Shutdown the server
				Running = false;
			}
		}

    }
}
