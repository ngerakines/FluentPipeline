namespace FluentPipeline.Core
{

    using Microsoft.Extensions.Logging;

    public interface IDispatcher
    {
        void Run();
        void StartWorker(ILoggerFactory logger);
        void Stop();
        void Wait();
        void StopAll();
        void WaitAll();
    }
}
