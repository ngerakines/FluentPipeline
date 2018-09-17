namespace FluentPipeline.Core.String
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Concurrent;

    public class StringWorkerFactory : IWorkerFactory<string>
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        public StringWorkerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.WorkerFactory");
        }

        public IWorker Create(IProducerConsumerCollection<string> workQueue)
        {
            return new StringWorker(loggerFactory, new RandomBackoffPolicy(), workQueue);
        }

    }
}