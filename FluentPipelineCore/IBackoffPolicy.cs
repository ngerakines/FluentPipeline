namespace FluentPipeline.Core
{
    public interface IBackoffPolicy
    {
        void RecordAttempt(bool success = false);

        int Delay();
    }
}