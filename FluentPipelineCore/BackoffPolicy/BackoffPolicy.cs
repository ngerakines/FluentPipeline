namespace FluentPipeline.Core
{
    using System.Collections.Generic;

    public class BackoffPolicy : IBackoffPolicy
    {
        private int attempts;
        private readonly int defaultBackoff;
        private IDictionary<int, int> delayMapping;

        public BackoffPolicy(int defaultBackoff = 5000)
        {
            this.defaultBackoff = defaultBackoff;
            delayMapping = new Dictionary<int, int>();
        }

        public void AddRule(int attempt, int delay)
        {
            delayMapping.Add(attempt, delay);
        }

        public void RecordAttempt(bool success = false)
        {
            if (success)
            {
                attempts = 0;
            }
            else
            {
                attempts++;
            }
        }

        public int Delay()
        {
            int result;
            if (delayMapping.TryGetValue(attempts, out result))
            {
                return result;
            }
            return defaultBackoff;
        }
    }
}