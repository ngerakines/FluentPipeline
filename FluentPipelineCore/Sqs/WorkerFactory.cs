namespace FluentPipeline.Core.Sqs
{
    using Amazon.SQS.Model;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;

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
}