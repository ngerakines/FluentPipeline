namespace FluentPipeline.Cli
{
    using System;
    using Microsoft.Extensions.Logging;

    using FluentPipeline.Core;

    class Program
    {

        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Debug);

            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("Program starting");

            var workerFactory = new StringWorkerFactory(loggerFactory);
            var dispatcherFactory = new StringDispatcherFactory(loggerFactory, workerFactory);
            var dispatcher = dispatcherFactory.Create();

            dispatcher.Run();
            dispatcher.StartWorker();

            while (true)
            {
                string s = Console.ReadLine();

                if (s == "q")
                {
                    break;
                }

                System.Threading.Thread.Sleep(100);
            }

            dispatcher.Stop();
            dispatcher.StopAll();
            dispatcher.Wait();
            dispatcher.WaitAll();

            logger.LogInformation("Program stopping");

        }
    }
}
