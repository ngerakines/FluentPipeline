namespace FluentPipeline.Core
{
    public interface IWorker
    {
        void Run();
        void Cancel();
    }
}
