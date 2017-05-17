namespace FluentPipeline.Core
{
    class LoggingEvents
    {
        public const int DISPATCHER_STARTUP = 1000;
        public const int DISPATCHER_SHUTDOWN = 1001;
        public const int DISPATCHER_RUN = 1002;
        public const int WORKER_STARTUP = 2000;
        public const int WORKER_SHUTDOWN = 2001;
        public const int WORKER_RUN = 2002;
    }
}
