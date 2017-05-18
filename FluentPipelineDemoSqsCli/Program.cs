namespace FluentPipeline.SqsCli
{
    using FluentPipeline.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;
    using FluentPipeline.Core.Middleware;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    class LogEvents
    {
        public const int ECHOMIDDLEWARE_RUN = 1000;
    }

    class EchoMiddleware : IMiddleware
    {
        private readonly ILogger logger;

        public EchoMiddleware(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.SqsCli.EchoMiddleware");
        }
        public bool CanHandle(JObject o)
        {
            return true;
        }

        public Task Handle(JObject o)
        {
            return Task.Run(() =>
            {
                logger.LogDebug(LogEvents.ECHOMIDDLEWARE_RUN, "Processing event.");
                logger.LogDebug(LogEvents.ECHOMIDDLEWARE_RUN, o.ToString());
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            var configration = builder.Build();

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Trace).AddDebug();

            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("Program starting");

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            serviceCollection.AddOptions();

            serviceCollection.AddTransient<IMiddleware, EchoMiddleware>();
            serviceCollection.AddTransient<IMiddlewareDispatcher, DefaultMiddlewareDispatcher>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var sqsDispatcherConfig = new SqsDispatcherConfiguration
            {
                AccessKey = configration.GetValue<string>("Sqs:AccessKey"),
                SecretKey = configration.GetValue<string>("Sqs:SecretKey"),
                Region = configration.GetValue<string>("Sqs:Region"),
                Queue = configration.GetValue<string>("Sqs:QueueUrl"),
                BacklogSize = configration.GetValue<int>("Sqs:BacklogSize")
            };

            var workerFactory = new SqsWorkerFactory(loggerFactory, serviceProvider, sqsDispatcherConfig);
            var backoffPolicy = new DefaultBackoffPolicy();
            var dispatcherFactory = new SqsDispatcherFactory(loggerFactory, workerFactory, backoffPolicy, sqsDispatcherConfig);
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
