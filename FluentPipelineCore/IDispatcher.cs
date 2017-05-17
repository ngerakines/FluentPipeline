namespace FluentPipeline.Core
{

    public interface IDispatcher<T>
    {
        void Run();
        void StartWorker();
        void Stop();
        void Wait();
        void StopAll();
        void WaitAll();
    }
}
