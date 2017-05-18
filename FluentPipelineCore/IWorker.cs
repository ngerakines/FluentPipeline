namespace FluentPipeline.Core
{
    public interface IWorker
    {
        void Run();
        void Stop();
        void Wait();
    }

}
