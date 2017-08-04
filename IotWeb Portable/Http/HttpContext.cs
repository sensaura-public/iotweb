using System.Collections.Generic;
using IotWeb.Common.Util;

namespace IotWeb.Common.Http
{
    public class HttpContext : CaseInsensitiveDictionary<object>
    {
        /// <summary>
        /// Common name for session entry.
        /// </summary>
        public const string Session = "Session";

        public SessionHandler SessionHandler;
    }
}
