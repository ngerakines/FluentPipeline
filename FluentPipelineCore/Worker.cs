namespace FluentPipeline.Core
{
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;

    class StringWorker : DelegatingWorker<string>
    {

        private readonly ILogger logger;

        public StringWorker(ILoggerFactory loggerFactory, IProducerConsumerCollection<string> workQueue) : base(loggerFactory, workQueue)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.StringWorker");
        }

        protected override void ProcessWork(string result)
        {
            logger.LogDebug(LoggingEvents.WORKER_RUN, "Worker has found work: {0}", result);
        }
    }
}
