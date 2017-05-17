namespace FluentPipeline.Core
{

    using System;
    using Microsoft.Extensions.Logging;
    using Amazon.SQS;
    using Amazon;
    using Amazon.SQS.Model;
    using Newtonsoft.Json;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.DependencyInjection;

    public class SqsDispatcherConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string Queue { get; set; }
        public int BacklogSize { get; set; }
    }

    public class SqsDispatcherFactory : IDispatcherFactory<Message>
    {

        private readonly SqsDispatcherConfiguration sqsDispatcherConfiguration;
        private readonly ILoggerFactory loggerFactory;
        private readonly IWorkerFactory<Message> workerFactory;

        public SqsDispatcherFactory(ILoggerFactory loggerFactory, IWorkerFactory<Message> workerFactory, SqsDispatcherConfiguration sqsDispatcherConfiguration)
        {
            this.sqsDispatcherConfiguration = sqsDispatcherConfiguration;
            this.loggerFactory = loggerFactory;
            this.workerFactory = workerFactory;
        }

        public IDispatcher<Message> Create()
        {
            return new SqsDispatcher(loggerFactory, workerFactory, sqsDispatcherConfiguration);
        }
    }

    public class SqsDispatcher : DelegatingDispatcher<Message>
    {
        private readonly ILogger logger;
        private readonly SqsDispatcherConfiguration sqsDispatcherConfiguration;

        public SqsDispatcher(ILoggerFactory loggerFactory, IWorkerFactory<Message> workerFactory, SqsDispatcherConfiguration sqsDispatcherConfiguration) : base(loggerFactory, workerFactory)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.SqsDispatcher");
            this.sqsDispatcherConfiguration = sqsDispatcherConfiguration;
        }

        public override void PopulateWork()
        {
            IAmazonSQS sqs = new AmazonSQSClient(RegionEndpoint.GetBySystemName(sqsDispatcherConfiguration.Region));
            var response = sqs.ReceiveMessageAsync(sqsDispatcherConfiguration.Queue).Result;

            foreach (Message message in response.Messages)
            {
                workQueue.Enqueue(message);
            }
        }
    }

    public class SqsWorkerFactory : IWorkerFactory<Message>
    {
        private readonly IServiceProvider services;
        private readonly ILoggerFactory loggerFactory;
        private readonly IWorkerFactory<Message> workerFactory;

        public SqsWorkerFactory(ILoggerFactory loggerFactory, IServiceProvider services)
        {
            this.services = services;
            this.loggerFactory = loggerFactory;
        }

        public IWorker Create(IProducerConsumerCollection<Message> workQueue)
        {
            return new SqsWorker(loggerFactory, workQueue, services);
        }
    }

    public class SqsWorker : DelegatingWorker<Message>
    {
        private readonly ILogger logger;
        private readonly IServiceProvider services;

        public SqsWorker(ILoggerFactory loggerFactory, IProducerConsumerCollection<Message> workQueue, IServiceProvider services) : base(loggerFactory, workQueue)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.SqsWorker");
            this.services = services;
        }

        protected override void ProcessWork(Message result)
        {
            logger.LogDebug(LoggingEvents.WORKER_RUN, "Worker has found work: {0}", result);
            using (var scopedServices = services.CreateScope())
            {
                var dispatcher = scopedServices.ServiceProvider.GetRequiredService<Object>();
                /* ... */
            }
        }
    }
}
