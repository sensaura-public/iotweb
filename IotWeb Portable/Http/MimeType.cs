using System.IO;
using System.Reflection;
using System.Collections.Generic;
using IotWeb.Common.Util;

namespace IotWeb.Common.Http
{
    public static class MimeType
    {
        // Default type to use if not known
        public const string DEFAULT = "application/x-binary";

        // Mimetype database
        private static Dictionary<string, string> m_mimeTypes = new Dictionary<string, string>();

        static MimeType()
        {
            // Load the database from resources
            Assembly assembly = Utilities.GetContainingAssembly(typeof(MimeType));
            StreamReader source = new StreamReader(assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources.mimetypes.txt"));
            char[] separators = new char[] { ',' };
            while (!source.EndOfStream)
            {
                string line = source.ReadLine();
                line = line.Trim();
                if ((line.Length == 0) || line.StartsWith("#"))
                    continue;
                string[] parts = line.Split(separators);
                if (parts.Length == 2)
                    m_mimeTypes[parts[0].Trim().ToLower()] = parts[1].Trim();
            }
        }

        /// <summary>
        /// Determine the mime type based on the extension or filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FromExtension(string filename)
        {
            int lastDot = filename.LastIndexOf('.');
            if (lastDot > 0)
                filename = filename.Substring(lastDot);
            string mimetype;
            if (!m_mimeTypes.TryGetValue(filename.ToLower(), out mimetype))
                mimetype = DEFAULT;
            return mimetype;
        }
    }
}
