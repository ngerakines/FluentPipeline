namespace FluentPipeline.Core
{
    using Microsoft.Extensions.Logging;
    public interface IDispatcher
    {
        void Run();
        void Cancel();
        void StartWorker(ILoggerFactory logger);
    }
}
