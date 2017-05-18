namespace FluentPipeline.Core
{

    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using System.Threading;

    public class RandomBackoffPolicy : IBackoffPolicy
    {
        private readonly Random random = new Random();
        private readonly int min;
        private readonly int max;

        public RandomBackoffPolicy(int min = 0, int max = 1000)
        {
            this.min = min;
            this.max = max;
        }

        public int Delay()
        {
            return random.Next(0, 1000);
        }

        public void RecordAttempt(bool success = false)
        {
            throw new NotImplementedException();
        }
    }

    public class StringDispatcher : DelegatingDispatcher<string>
    {
        private readonly ILogger logger;
        private readonly Random random = new Random();

        public StringDispatcher(ILoggerFactory loggerFactory, IWorkerFactory<string> workerFactory, IBackoffPolicy backoffPolicy) : base(loggerFactory, workerFactory, backoffPolicy)
        {
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.StringDispatcher");
        }

        public override bool PopulateWork(CancellationToken cancellationToken)
        {
            #region demo
            logger.LogInformation(LoggingEvents.DISPATCHER_RUN, "Dispatcher queing work.");
            #endregion

            return true;
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
