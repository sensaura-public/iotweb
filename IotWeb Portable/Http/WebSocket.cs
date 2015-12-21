using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
	public class WebSocket
	{
		// Instance variables
		private Stream m_input;
		private Stream m_output;

		/// <summary>
		/// Internal constructor.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		internal WebSocket(Stream input, Stream output)
		{

		}

		/// <summary>
		/// Process messages until the socket is closed.
		/// </summary>
		internal void Run()
		{
		}
	}
}
