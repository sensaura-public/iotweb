using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common
{
	public delegate void ConnectionHandler(ISocketServer sender, string hostname, Stream input, Stream output);

	public interface ISocketServer : IServer
	{
        /// <summary>
        /// Delegate invoked when a new connection is accepted
        /// </summary>
		ConnectionHandler ConnectionRequested { get; set; }

        /// <summary>
        /// Helper method to read from an input stream with timeout support
        /// 
        /// This is a horrible way to do it but due to the way various classes
        /// are hidden in a PCL we need to implement it somwhere where access
        /// to those classes are available.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="timedOut"></param>
        /// <returns></returns>
        int ReadWithTimeout(Stream input, byte[] buffer, int offset, int count, out bool timedOut);
	}
}
