using System;
using System.IO;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using IotWeb.Common;

namespace IotWeb.Server
{
    public class SocketServer : ISocketServer
    {
        // Instance variables
        private bool m_running;
        private ConnectionHandler m_handler;
        private List<StreamSocketListener> m_listeners;

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

        public SocketServer()
        {
            m_running = false;
        }

        public async void Start(int port)
        {
            lock (this)
            {
                if (m_running)
                    throw new InvalidOperationException("Server is already running.");
                m_running = true;
            }
            Port = port;
            m_listeners = new List<StreamSocketListener>();
            StreamSocketListener listener;
            foreach (HostName candidate in NetworkInformation.GetHostNames())
            {
                if ((candidate.Type == HostNameType.Ipv4) || (candidate.Type == HostNameType.Ipv6))
                {
                    listener = new StreamSocketListener();
                    listener.ConnectionReceived += OnConnectionReceived;
                    await listener.BindEndpointAsync(candidate, Port.ToString());
                    m_listeners.Add(listener);
                }
            }
        }

        public void Stop()
        {
            lock(this)
            {
                if (!m_running)
                    return;
                m_running = false;
                // Clean up all listeners
                foreach (StreamSocketListener listener in m_listeners)
                    listener.Dispose();
                m_listeners.Clear();
                m_listeners = null;
            }
        }

        private void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if ((m_handler != null) && m_running)
            {
                IAsyncAction asyncAction = ThreadPool.RunAsync((workItem) =>
                    {
                        StreamSocket s = args.Socket;
                        try
                        {
                            m_handler(
                                this,
                                s.Information.RemoteHostName.CanonicalName.ToString(),
                                s.InputStream.AsStreamForRead(),
                                s.OutputStream.AsStreamForWrite()
                                );
                        }
                        catch (Exception)
                        {
                            // Quietly consume the exception
                        }
                        // Close the client socket
                        s.Dispose();
                    });
            }
        }

    }
}
