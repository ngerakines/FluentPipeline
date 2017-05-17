namespace FluentPipeline.Core
{

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    class Dispatcher : IDispatcher
    {
        private readonly ILogger logger;
        private readonly Concurrent​Queue<string> workQueue;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        private readonly IList<Worker> workers;
        private readonly IList<Task> tasks;

        public Dispatcher(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.Dispatcher");
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            workQueue = new Concurrent​Queue<string>();
            tasks = new List<Task>();
            workers = new List<Worker>();
        }

        public void Cancel()
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
            foreach (var worker in workers)
            {
                worker.Cancel();
            }
        }

        public void Run()
        {
            Task.Run(() =>
            {
                logger.LogInformation(LoggingEvents.DISPATCHER_STARTUP, "Dispatcher has started.");
                Random rnd = new Random();
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        #region demo
                        logger.LogInformation(LoggingEvents.DISPATCHER_RUN, "Dispatcher queing work.");
                        workQueue.Enqueue(RandomString(4));

                        var sleepDuration = random.Next(0, 1000);
                        logger.LogDebug(LoggingEvents.DISPATCHER_RUN, "Dispatcher sleeping. amount={0}", sleepDuration);
                        Task.Delay(sleepDuration, cancellationToken).Wait();
                        #endregion
                    }
                    catch (TaskCanceledException)
                    { }
                }
                logger.LogInformation(LoggingEvents.DISPATCHER_SHUTDOWN, "Dispatcher has shutdown.");
            });
        }

        public void StartWorker(ILoggerFactory logger)
        {
            var worker = new Worker(logger, workQueue);
            workers.Add(worker);
            worker.Run();
        }

        #region demo
        private readonly Random random = new Random();
        private readonly object syncLock = new object();

        private string RandomString(int Size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < Size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
        #endregion

    }
}
