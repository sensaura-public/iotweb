using System;
using System.IO;
using System.Collections.Generic;

namespace IotWeb.Common.Http
{
	public class BaseHttpServer : IServer
	{
		// Instance variables
        private List<IHttpFilter> m_filters;
        private Dictionary<string, IHttpRequestHandler> m_handlers;
		private Dictionary<string, IWebSocketRequestHandler> m_wsHandlers;

        internal ISocketServer SocketServer { get; private set; }

		public event ServerStoppedHandler ServerStopped;

		public bool Running { get { return SocketServer.Running; } }

		protected BaseHttpServer(ISocketServer server)
		{
			SocketServer = server;
			SocketServer.ConnectionRequested = ConnectionRequested;
			SocketServer.ServerStopped += OnServerStopped;
            m_filters = new List<IHttpFilter>();
            m_handlers = new Dictionary<string, IHttpRequestHandler>();
			m_wsHandlers = new Dictionary<string, IWebSocketRequestHandler>();
		}

		private void OnServerStopped(IServer server)
		{
			ServerStoppedHandler handler = ServerStopped;
			if (handler != null)
				handler(this);
		}

		public void Start()
		{
			SocketServer.Start();
		}

		public void Stop()
		{
			SocketServer.Stop();
		}

		/// <summary>
		/// Add a IHttpFilter instance.
		/// 
		/// Filters are applied in the order they are added.
		/// </summary>
		/// <param name="filter"></param>
        public void AddHttpFilter(IHttpFilter filter)
        {
            lock(m_filters)
            {
                m_filters.Add(filter);
            }
        }

		public void AddWebSocketRequestHandler(string uri, IWebSocketRequestHandler handler)
		{
			// TODO: Verify URI
			lock (m_wsHandlers)
			{
				m_wsHandlers.Add(uri, handler);
			}
		}

		/// <summary>
		/// Map an IHttpRequestHandler instance to a URI
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="handler"></param>
        public void AddHttpRequestHandler(string uri, IHttpRequestHandler handler)
        {
            // TODO: Verify URI
            lock (m_handlers)
            {
                m_handlers.Add(uri, handler);
            }
        }

		/// <summary>
		/// Handle the HTTP connection
		/// 
		/// This implementation doesn't support keep alive so each HTTP session
		/// consists of parsing the request, dispatching to a handler and then
		/// sending the response before closing the connection.
		/// </summary>
		/// <param name="server"></param>
		/// <param name="hostname"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		private void ConnectionRequested(ISocketServer server, string hostname, Stream input, Stream output)
		{
			HttpRequestProcessor processor = new HttpRequestProcessor(this);
			processor.ProcessHttpRequest(input, output);
		}

        /// <summary>
        /// Apply all the filters to the current request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        internal bool ApplyBeforeFilters(HttpRequest request, HttpResponse response, HttpContext context)
        {
			bool allowHandling = true;
            lock (m_filters)
            {
				foreach (IHttpFilter filter in m_filters)
				{
					try
					{
						allowHandling = allowHandling && filter.Before(request, response, context);
					}
					catch (Exception)
					{
						// Just silently ignore it
					}
				}
            }
			return allowHandling;
        }

		/// <summary>
		/// Apply all the filters to the current request
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <param name="context"></param>
		internal void ApplyAfterFilters(HttpRequest request, HttpResponse response, HttpContext context)
		{
			lock (m_filters)
			{
				foreach (IHttpFilter filter in m_filters)
				{
					try
					{
						filter.After(request, response, context);
					}
					catch (Exception)
					{
						// Just silently ignore it
					}
				}
			}
		}

        /// <summary>
        /// Find the matching handler for the request
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="partialUri"></param>
        /// <returns></returns>
        internal IHttpRequestHandler GetHandlerForUri(string uri, out string partialUri)
        {
            partialUri = uri;
            int length = 0;
            IHttpRequestHandler handler = null;
            lock (m_handlers)
            {
                // Find the longest match
                foreach (string mapped in m_handlers.Keys)
                {
                    if (uri.StartsWith(mapped) && (mapped.Length > length))
                    {
                        length = mapped.Length;
                        handler = m_handlers[mapped];
                        partialUri = uri.Substring(length);
                    }
                }
            }
            return handler;
        }

		/// <summary>
		/// Find the matching handler for the WebSocket request
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="partialUri"></param>
		/// <returns></returns>
		internal IWebSocketRequestHandler GetHandlerForWebSocket(string uri, out string partialUri)
		{
			partialUri = uri;
			int length = 0;
			IWebSocketRequestHandler handler = null;
			lock (m_wsHandlers)
			{
				// Find the longest match
				foreach (string mapped in m_wsHandlers.Keys)
				{
					if (uri.StartsWith(mapped) && (mapped.Length > length))
					{
						length = mapped.Length;
						handler = m_wsHandlers[mapped];
						partialUri = uri.Substring(length);
					}
				}
			}
			return handler;
		}


	}
}
