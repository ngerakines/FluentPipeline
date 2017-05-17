namespace FluentPipeline.Core
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    class Worker
    {
        private readonly ILogger logger;
        private readonly IProducerConsumerCollection<string> workQueue;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;

        public Worker(ILoggerFactory loggerFactory, IProducerConsumerCollection<string> workQueue)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.Worker");
            this.workQueue = workQueue;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void Cancel()
        {
            logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Worker shutdown has been requested.");
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Worker can be shutdown.");
                cancellationTokenSource.Cancel();
            }
            else
            {
                logger.LogWarning(LoggingEvents.WORKER_SHUTDOWN, "Worker is already shutting down.");
            }
        }


        public void Run()
        {
            Task.Run(() =>
            {
                logger.LogInformation(LoggingEvents.WORKER_STARTUP, "Worker has started.");
                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation(LoggingEvents.WORKER_RUN, "Worker is looking for work.");
                    try
                    {
                        string result;
                        if (workQueue.TryTake(out result))
                        {
                            logger.LogDebug(LoggingEvents.WORKER_RUN, "Worker has found work: {0}", result);
                        }
                        else
                        {
                            logger.LogInformation(LoggingEvents.WORKER_RUN, "Worker did not find work.");
                            Task.Delay(5000, cancellationToken).Wait();
                        }
                    }
                    catch (TaskCanceledException)
                    { }
                }
                logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Worker has shutdown.");
            });
        }
    }
}
