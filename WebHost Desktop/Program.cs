using System;
using System.ComponentModel;
using System.Threading;
using IotWeb.Server;
using IotWeb.Common.Util;
using IotWeb.Common.Http;
using Splat;

namespace WebHost.Desktop
{
	class Program
	{
        class ConsoleLogger : ILogger
        {
            public LogLevel Level { get; set; }

            public void Write([Localizable(false)] string message, LogLevel logLevel)
            {
                Console.WriteLine("{0}: {1}", logLevel, message);
            }
        }

        static void Main(string[] args)
		{
            // Show the logs
            ILogger logger = new ConsoleLogger();
            logger.Level = LogLevel.Debug;
            Locator.CurrentMutable.RegisterConstant(logger, typeof(ILogger));
            // Set up and run the server
			HttpServer server = new HttpServer();
            server.AddHttpRequestHandler(
                "/",
                new HttpResourceHandler(
                    Utilities.GetContainingAssembly(typeof(Program)),
                    "Resources.Site",
                    "index.html"
                    )
                );
			server.Start(8000);
            LogHost.Default.Debug("Server running - press any key to stop.");
            while (!Console.KeyAvailable)
                Thread.Sleep(100);
            Console.ReadKey();
		}
	}
}
