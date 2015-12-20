using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using IotWeb.Common;
using Splat;

namespace IotWeb.Server
{
    public class SocketServer : ISocketServer, IEnableLogger
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
            // Bind to localhost as well
            listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnectionReceived;
            await listener.BindEndpointAsync(new HostName("localhost"), Port.ToString());
            m_listeners.Add(listener);
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
            try
            {
                if ((m_handler != null) && m_running)
                {
                    m_handler(
                        this,
                        args.Socket.Information.RemoteHostName.CanonicalName.ToString(),
                        args.Socket.InputStream.AsStreamForRead(),
                        args.Socket.OutputStream.AsStreamForWrite()
                        );
                }
            }
            catch (Exception ex)
            {
                this.Log().Debug("Unexpected error processing request - {0}", ex.Message);
            }
            // Close the client socket
            args.Socket.Dispose();
        }

        public int ReadWithTimeout(Stream input, byte[] buffer, int offset, int count, out bool timedOut)
        {
            timedOut = false;
            try
            {
                int result = input.Read(buffer, offset, count);
                return result;
            }
            catch (IOException ex)
            {
                SocketException se = ex.InnerException as SocketException;
                if ((se != null) && (se.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut))
                {
                    timedOut = true;
                    return 0;
                }
                throw ex;
            }
        }
    }
}
