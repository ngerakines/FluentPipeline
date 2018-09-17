namespace FluentPipeline.Core.Sqs
{
    using Amazon.SQS.Model;
    using Microsoft.Extensions.Logging;

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
}