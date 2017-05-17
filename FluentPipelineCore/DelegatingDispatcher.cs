namespace FluentPipeline.Core
{

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    public abstract class DelegatingDispatcher<T> : IDispatcher<T>
    {
        private readonly ILogger logger;
        private readonly IWorkerFactory<T> workerFactory;
        private readonly CancellationTokenSource cancellationTokenSource;

        protected readonly Concurrent​Queue<T> workQueue;
        protected readonly IList<IWorker> workers;
        protected readonly CancellationToken cancellationToken;
        protected Task serviceTask;

        public DelegatingDispatcher(ILoggerFactory loggerFactory, IWorkerFactory<T> workerFactory)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.Dispatcher");
            this.workerFactory = workerFactory;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            workQueue = new Concurrent​Queue<T>();
            workers = new List<IWorker>();
        }

        public void Stop()
        {
            logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher shutdown has been requested.");
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher can be shutdown.");
                cancellationTokenSource.Cancel();
            }
            else
            {
                logger.LogWarning(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher is already shutting down.");
            }
        }

        public void StopAll()
        {
            foreach (var worker in workers)
            {
                worker.Stop();
            }
        }

        public void Wait()
        {
            logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Waiting for dispatcher to stop.");
            try
            {
                if (!serviceTask.Wait(TimeSpan.FromSeconds(30)))
                {
                    logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher did not gracefully stop.");
                }
                else
                {
                    logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher gracefully stopped.");
                }
            }
            catch (System.Exception)
            {
                // Ignored
            }
        }

        public void WaitAll()
        {
            foreach (var worker in workers)
            {
                worker.Wait();
            }
        }

        public abstract void PopulateWork();

        public void Run()
        {
            serviceTask = Task.Run(() =>
            {
                logger.LogInformation(LoggingEvents.DISPATCHER_STARTUP, "Dispatcher has started.");
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        PopulateWork();
                    }
                    catch (TaskCanceledException)
                    { }
                }
                logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher has shutdown.");
            });
        }

        public void StartWorker()
        {
            var worker = workerFactory.Create(workQueue);
            workers.Add(worker);
            worker.Run();
        }

    }
}
