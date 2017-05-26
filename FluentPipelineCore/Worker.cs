namespace FluentPipeline.Core
{
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;

    class StringWorker : DelegatingWorker<string>
    {

        private readonly ILogger logger;

        public StringWorker(ILoggerFactory loggerFactory, IBackoffPolicy backoffPolicy, IProducerConsumerCollection<string> workQueue, string workerName = null) : base(loggerFactory, backoffPolicy, workQueue)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.StringWorker");
        }

        protected override void ProcessWork(string result)
        {
            logger.LogDebug(LoggingEvents.WORKER_RUN, "Worker has found work: {0}", result);
        }
    }
}
