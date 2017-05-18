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
    using FluentPipeline.Core.Middleware;
    using System.Threading;

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
        private readonly IBackoffPolicy backoffPolicy = null;

        public SqsDispatcherFactory(ILoggerFactory loggerFactory, IWorkerFactory<Message> workerFactory, IBackoffPolicy backoffPolicy, SqsDispatcherConfiguration sqsDispatcherConfiguration)
        {
            this.sqsDispatcherConfiguration = sqsDispatcherConfiguration;
            this.loggerFactory = loggerFactory;
            this.workerFactory = workerFactory;
            this.backoffPolicy = backoffPolicy;
        }

        public IDispatcher<Message> Create()
        {
            return new SqsDispatcher(loggerFactory, workerFactory, backoffPolicy, sqsDispatcherConfiguration);
        }
    }

    public class SqsDispatcher : DelegatingDispatcher<Message>
    {
        private readonly ILogger logger;
        private readonly SqsDispatcherConfiguration sqsDispatcherConfiguration;

        public SqsDispatcher(ILoggerFactory loggerFactory, IWorkerFactory<Message> workerFactory, IBackoffPolicy backoffPolicy, SqsDispatcherConfiguration sqsDispatcherConfiguration) : base(loggerFactory, workerFactory, backoffPolicy)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.SqsDispatcher");
            this.sqsDispatcherConfiguration = sqsDispatcherConfiguration;
        }

        public override bool PopulateWork(CancellationToken cancellationToken)
        {
            var success = false;
            IAmazonSQS sqs = new AmazonSQSClient(sqsDispatcherConfiguration.AccessKey, sqsDispatcherConfiguration.SecretKey, RegionEndpoint.GetBySystemName(sqsDispatcherConfiguration.Region));
            var response = sqs.ReceiveMessageAsync(sqsDispatcherConfiguration.Queue).Result;

            logger.LogDebug(LoggingEvents.DISPATCHER_RUN, "Received messages from SQS. count={0} queue={1}", response.Messages.Count, sqsDispatcherConfiguration.Queue);

            foreach (Message message in response.Messages)
            {
                logger.LogDebug(LoggingEvents.DISPATCHER_RUN, "Populating queue with message. id={0} queue={1}", message.MessageId, sqsDispatcherConfiguration.Queue);
                logger.LogTrace(LoggingEvents.DISPATCHER_RUN, message.Body);
                workQueue.Enqueue(message);
                success = true;
            }

            return success;
        }
    }

    public class SqsWorkerFactory : IWorkerFactory<Message>
    {
        private readonly IServiceProvider services;
        private readonly ILoggerFactory loggerFactory;
        private readonly IBackoffPolicy backoffPolicy;
        private readonly SqsDispatcherConfiguration sqsDispatcherConfiguration;

        public SqsWorkerFactory(ILoggerFactory loggerFactory, IServiceProvider services, IBackoffPolicy backoffPolicy, SqsDispatcherConfiguration sqsDispatcherConfiguration)
        {
            this.services = services;
            this.loggerFactory = loggerFactory;
            this.backoffPolicy = backoffPolicy;
            this.sqsDispatcherConfiguration = sqsDispatcherConfiguration;
        }

        public IWorker Create(IProducerConsumerCollection<Message> workQueue)
        {
            return new SqsWorker(loggerFactory, workQueue, services, backoffPolicy, sqsDispatcherConfiguration);
        }
    }

    public class SqsWorker : DelegatingWorker<Message>
    {
        private readonly ILogger logger;
        private readonly IServiceProvider services;
        private readonly SqsDispatcherConfiguration sqsDispatcherConfiguration;
        private readonly IBackoffPolicy backoffPolicy;

        public SqsWorker(ILoggerFactory loggerFactory, IProducerConsumerCollection<Message> workQueue, IServiceProvider services, IBackoffPolicy backoffPolicy, SqsDispatcherConfiguration sqsDispatcherConfiguration) : base(loggerFactory, backoffPolicy, workQueue)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.SqsWorker");
            this.services = services;
            this.backoffPolicy = backoffPolicy;
            this.sqsDispatcherConfiguration = sqsDispatcherConfiguration;
        }

        protected override void ProcessWork(Message message)
        {
            logger.LogDebug(LoggingEvents.WORKER_RUN, "Worker has found work: {0}", message);
            using (var scopedServices = services.CreateScope())
            {
                var sqs = scopedServices.ServiceProvider.GetRequiredService<IAmazonSQS>();
                try
                {
                    var wrappedSnsMessage = JsonConvert.DeserializeObject<WrappedSnsMessage>(message.Body);
                    var dispatcher = scopedServices.ServiceProvider.GetRequiredService<IMiddlewareDispatcher>();
                    dispatcher.Dispatch(wrappedSnsMessage.Message);
                    sqs.DeleteMessageAsync(sqsDispatcherConfiguration.Queue, message.ReceiptHandle).Wait();
                }
                catch (Exception e)
                {
                    logger.LogError(0, e, "Error processing message");
                }
            }
        }
    }

    class WrappedSnsMessage
    {
        public String Type { get; set; }
        public String MessageId { get; set; }
        public String TopicArn { get; set; }
        public String Message { get; set; }
        public String Timestamp { get; set; }
        public String SignatureVersion { get; set; }
        public String Signature { get; set; }
        public String SigningCertURL { get; set; }
        public String UnsubscribeURL { get; set; }
    }
}
