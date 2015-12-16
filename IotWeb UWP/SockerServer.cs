using System;
using System.IO;
using Windows.Networking;
using Windows.Networking.Sockets;
using IotWeb.Common;
using Splat;

namespace IotWeb.Server
{
    public class SockerServer : ISocketServer, IEnableLogger
    {
        // Instance variables
        private bool m_running;
        private ConnectionHandler m_handler;
        private StreamSocketListener m_listener;

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

        public SockerServer()
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
            m_listener = new StreamSocketListener();
            m_listener.ConnectionReceived += OnConnectionReceived;
            await m_listener.BindEndpointAsync(new HostName("0.0.0.0"), Port.ToString());
        }

        public void Stop()
        {
            lock(this)
            {
                if (!m_running)
                    return;
                m_listener.Dispose();
                m_running = false;
            }
        }

        private void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                if (m_handler != null)
                {
                    m_handler(
                        this,
                        args.Socket.Information.RemoteHostName.CanonicalName.ToString(),
                        args.Socket.InputStream.AsStreamForRead(),
                        args.Socket.OutputStream.AsStreamForWrite()
                        );
                }
                else
                    this.Log().Debug("No handler provided for connection requests.");
            }
            catch (Exception ex)
            {
                this.Log().Debug("Unexpected error processing request - {0}", ex.Message);
            }
            // Close the client socket
            args.Socket.Dispose();
        }
    }
}
