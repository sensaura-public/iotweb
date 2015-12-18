using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
    public interface IWebSocketRequestHandler
    {
        bool WillAcceptRequest(string uri, string protocol);
    }
}
