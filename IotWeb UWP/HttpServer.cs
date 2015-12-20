using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common.Http;

namespace IotWeb.Server
{
    public class HttpServer : BaseHttpServer
    {
        public HttpServer()
            : base(new SocketServer())
        {
            // No configuration required
        }
    }
}
