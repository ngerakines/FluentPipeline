namespace FluentPipeline.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    public abstract class DelegatingWorker<T> : IWorker
    {
        private readonly ILogger logger;
        private readonly IProducerConsumerCollection<T> workQueue;
        private readonly IBackoffPolicy backoffPolicy;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        private Task serviceTask;

        public DelegatingWorker(ILoggerFactory loggerFactory, IBackoffPolicy backoffPolicy, IProducerConsumerCollection<T> workQueue)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.DelegatingWorker");
            this.workQueue = workQueue;
            this.backoffPolicy = backoffPolicy;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        protected abstract void ProcessWork(T result);

        public void Run()
        {
            serviceTask = Task.Run(() =>
            {
                var backoffPolicy = new GraduatedBackoffPolicy();

                logger.LogInformation(LoggingEvents.WORKER_STARTUP, "Worker has started.");
                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation(LoggingEvents.WORKER_RUN, "Worker is looking for work.");
                    try
                    {
                        T result;
                        if (workQueue.TryTake(out result))
                        {
                            logger.LogDebug(LoggingEvents.WORKER_RUN, "Worker has found work: {0}", result);
                            ProcessWork(result);

                            backoffPolicy.RecordAttempt(true);
                        }
                        else
                        {
                            logger.LogInformation(LoggingEvents.WORKER_RUN, "Worker did not find work; preparing to wait={0}.", backoffPolicy.Delay());

                            backoffPolicy.RecordAttempt(false);

                            Task.Delay(backoffPolicy.Delay(), cancellationToken).Wait();
                        }
                    }
                    catch (TaskCanceledException)
                    { }
                }
                logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Worker has shutdown.");
            });
        }

        public void Stop()
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

        public void Wait()
        {
            logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Waiting for worker to stop.");
            try
            {
                if (!serviceTask.Wait(TimeSpan.FromSeconds(30)))
                {
                    logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Worker did not gracefully stop.");
                }
                else
                {
                    logger.LogInformation(LoggingEvents.WORKER_SHUTDOWN, "Worker gracefully stopped.");
                }
            }
            catch (System.Exception)
            {
                // Ignored
            }
        }
    }
}
