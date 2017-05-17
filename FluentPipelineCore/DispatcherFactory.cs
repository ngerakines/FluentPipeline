namespace FluentPipeline.Core
{
    using Microsoft.Extensions.Logging;
    public class DispatcherFactory
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        public DispatcherFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger("FluentPipeline.Core.DispatcherFactory");
        }

        public IDispatcher Create()
        {
            return new Dispatcher(loggerFactory);
        }
    }
}
