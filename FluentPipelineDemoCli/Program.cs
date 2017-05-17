namespace FluentPipeline.Cli
{
    using System;
    using System.Threading;
    using Microsoft.Extensions.Logging;

    using FluentPipeline.Core;

    class Program
    {

        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            loggerFactory.AddConsole(Microsoft.Extensions.Logging.LogLevel.Debug);

            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("Program starting");

            var dispatcherFactory = new DispatcherFactory(loggerFactory);
            var dispatcher = dispatcherFactory.Create();

            dispatcher.Run();
            dispatcher.StartWorker(loggerFactory);

            while(true)
			{
				string s = Console.ReadLine();
				
				if(s == "q")
					break;
				
				System.Threading.Thread.Sleep(100);
			}

            dispatcher.Cancel();

            logger.LogInformation("Program stopping");
            
        }
    }
}
