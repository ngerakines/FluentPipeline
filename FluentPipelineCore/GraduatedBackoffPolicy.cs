namespace FluentPipeline.Core
{

    using System;
    public class GraduatedBackoffPolicy : IBackoffPolicy
    {
        private int attempts;
        private readonly int maxAttempts;
        private readonly int incrementValue;

        public GraduatedBackoffPolicy(int maxAttempts = 60, int incrementValue = 1000)
        {
            if(maxAttempts < 1)
            {
                throw new ArgumentException("Value must be greater than 1.", "maxAttempts");
            }

            if(incrementValue < 1)
            {
                throw new ArgumentException("Value must be greater than 1.", "incrementValue");
            }

            attempts = 1;
            this.maxAttempts = maxAttempts;
            this.incrementValue = incrementValue;
        }
        public int Delay()
        {
            return attempts * incrementValue;
        }

        public void RecordAttempt(bool success = false)
        {
            if (success)
            {
                attempts = 1;
            }
            else
            {
                attempts++;
            }

            attempts = (attempts > maxAttempts ? maxAttempts : attempts);
        }
    }

}
