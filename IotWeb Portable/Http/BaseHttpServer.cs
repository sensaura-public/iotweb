﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common;
using Splat;

namespace IotWeb.Common.Http
{
	public class BaseHttpServer : IServer, IEnableLogger
	{
		// Instance variables
        private List<IHttpFilter> m_filters;
        private Dictionary<string, IHttpRequestHandler> m_handlers;

        internal ISocketServer SocketServer { get; private set; }

        public int Port
		{
			get { return SocketServer.Port; }
		}

		protected BaseHttpServer(ISocketServer server)
		{
			SocketServer = server;
			SocketServer.ConnectionRequested = ConnectionRequested;
            m_filters = new List<IHttpFilter>();
            m_handlers = new Dictionary<string, IHttpRequestHandler>();
		}

		public void Start(int port)
		{
			SocketServer.Start(port);
		}

		public void Stop()
		{
			SocketServer.Stop();
		}

        public void AddHttpFilter(IHttpFilter filter)
        {
            lock(m_filters)
            {
                m_filters.Add(filter);
            }
        }

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
        internal void ApplyFilters(HttpRequest request, HttpResponse response, HttpContext context)
        {
            lock (m_filters)
            {
                foreach (IHttpFilter filter in m_filters)
                    filter.ApplyFilter(request, response, context);
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
	}
}