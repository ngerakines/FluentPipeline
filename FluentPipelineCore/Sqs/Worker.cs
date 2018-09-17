namespace FluentPipeline.Core.Sqs
{
    using Amazon.SQS;
    using Amazon.SQS.Model;
    using FluentPipeline.Core.Middleware;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;

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