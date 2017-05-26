namespace FluentPipeline.Core
{

    public interface IDispatcher<T>
    {
        void Run();
        void StartWorker(string workerName = null);
        void Stop();
        void Wait();
        void StopAll();
        void WaitAll();
    }
}
