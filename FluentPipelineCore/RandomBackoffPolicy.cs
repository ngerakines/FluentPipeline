namespace FluentPipeline.Core
{

    using System;
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
}
