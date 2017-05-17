namespace FluentPipeline.Core
{

    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class StringDispatcher : DelegatingDispatcher<string>
    {
        private readonly Random random = new Random();
        private readonly object syncLock = new object();

        private readonly ILogger logger;

        public StringDispatcher(ILoggerFactory loggerFactory, IWorkerFactory<string> workerFactory) : base(loggerFactory, workerFactory)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.StringDispatcher");
        }

        public override void PopulateWork()
        {
            #region demo
            logger.LogInformation(LoggingEvents.DISPATCHER_RUN, "Dispatcher queing work.");
            //workQueue.Enqueue(RandomString(4));

            var sleepDuration = random.Next(0, 1000);
            logger.LogDebug(LoggingEvents.DISPATCHER_RUN, "Dispatcher sleeping. amount={0}", sleepDuration);
            Task.Delay(sleepDuration, cancellationToken).Wait();
            #endregion
        }

        #region demo

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
