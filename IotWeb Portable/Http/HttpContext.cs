using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
    public class HttpContext : CaseInsensitiveDictionary<object>
    {
        /// <summary>
        /// Common name for session entry.
        /// </summary>
        public const string Session = "Session";
    }
}
