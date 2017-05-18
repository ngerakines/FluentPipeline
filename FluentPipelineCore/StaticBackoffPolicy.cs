namespace FluentPipeline.Core
{
    public class StaticBackoffPolicy : IBackoffPolicy
    {
        private readonly int delay;

        public StaticBackoffPolicy(int delay)
        {
            this.delay = delay;
        }

        public int Delay()
        {
            return delay;
        }

        public void RecordAttempt(bool success = false)
        {
            // noop
        }
    }
}
