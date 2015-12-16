using System;
using System.ComponentModel;
using System.Threading;
using IotWeb.Server;
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
            Locator.CurrentMutable.RegisterConstant(new ConsoleLogger(), typeof(ILogger));
            // Set up and run the server
			HttpServer server = new HttpServer();
			server.Start(8000);
            Console.WriteLine("Server running - press any key to stop.");
            while (!Console.KeyAvailable)
                Thread.Sleep(100);
            Console.ReadKey();
		}
	}
}
