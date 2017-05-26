namespace FluentPipeline.SqsCli
{
    using Amazon;
    using Amazon.Extensions.NETCore.Setup;
    using Amazon.Runtime;
    using Amazon.SQS;
    using FluentPipeline.Core;
    using FluentPipeline.Core.Middleware;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
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
            var configuration = builder.Build();

            var sqsDispatcherConfig = new SqsDispatcherConfiguration
            {
                AccessKey = configuration.GetValue<string>("Sqs:AccessKey"),
                SecretKey = configuration.GetValue<string>("Sqs:SecretKey"),
                Region = configuration.GetValue<string>("Sqs:Region"),
                Queue = configuration.GetValue<string>("Sqs:QueueUrl"),
                BacklogSize = configuration.GetValue<int>("Sqs:BacklogSize")
            };

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Trace).AddDebug();

            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("Program starting");

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            serviceCollection.AddOptions();

            serviceCollection.AddTransient<IMiddleware, EchoMiddleware>();
            serviceCollection.AddTransient<IMiddlewareDispatcher, DefaultMiddlewareDispatcher>();

            serviceCollection.AddAWSService<IAmazonSQS>(new AWSOptions
            {
                Region = RegionEndpoint.GetBySystemName(sqsDispatcherConfig.Region),
                Credentials = new BasicAWSCredentials(sqsDispatcherConfig.AccessKey, sqsDispatcherConfig.SecretKey)
            });

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var workerFactory = new SqsWorkerFactory(loggerFactory, serviceProvider, new GraduatedBackoffPolicy(), sqsDispatcherConfig);
            var dispatcherFactory = new SqsDispatcherFactory(loggerFactory, workerFactory, new StaticBackoffPolicy(5000), sqsDispatcherConfig);
            var dispatcher = dispatcherFactory.Create();

            dispatcher.Run();

            for(int i = 0; i < 5; i ++) // Create 5 workers
            {
                dispatcher.StartWorker("worker " + i.ToString());
            }

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
