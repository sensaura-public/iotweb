using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common
{
	public interface IServer
	{
		int Port { get; }

		void Start(int port);

		void Stop();
	}
}
