namespace FluentPipeline.Core
{
    using Microsoft.Extensions.Logging;

    public interface IDispatcherFactory<T>
    {
        IDispatcher<T> Create();
    }

    public class StringDispatcherFactory : IDispatcherFactory<string>
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly IWorkerFactory<string> workerFactory;

        public StringDispatcherFactory(ILoggerFactory loggerFactory, IWorkerFactory<string> workerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.workerFactory = workerFactory;
            logger = loggerFactory.CreateLogger("FluentPipeline.Core.DispatcherFactory");
        }

        public IDispatcher<string> Create()
        {
            logger.LogInformation("Creating dispatcher.");
            return new StringDispatcher(loggerFactory, workerFactory, null);
        }
    }
}
