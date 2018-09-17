namespace FluentPipeline.Core.Sqs
{
    using Amazon;
    using Amazon.SQS;
    using Amazon.SQS.Model;
    using Microsoft.Extensions.Logging;
    using System.Threading;

    public class SqsDispatcherConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string Queue { get; set; }
        public int BacklogSize { get; set; }
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
}