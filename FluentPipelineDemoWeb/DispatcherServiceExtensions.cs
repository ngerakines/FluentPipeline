namespace FluentPipeline.Web
{

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using FluentPipeline.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    public static class DispatcherServiceExtensions
    {

        public static void UseDispatcher(this IApplicationBuilder builder, ILoggerFactory loggerFactory, IConfigurationRoot configuration)
        {
            var lifetime = (IApplicationLifetime)builder.ApplicationServices.GetService(typeof(IApplicationLifetime));

            var stoppingToken = lifetime.ApplicationStopping;
            var stoppedToken = lifetime.ApplicationStopped;

            var workerFactory = new StringWorkerFactory(loggerFactory);
            var dispatcherFactory = new StringDispatcherFactory(loggerFactory, workerFactory);
            var dispatcher = dispatcherFactory.Create();

            dispatcher.Run();
            // How many workers do we start?
            dispatcher.StartWorker();

            stoppingToken.Register(() =>
            {
                dispatcher.Stop();
                dispatcher.StopAll();
            });

            stoppedToken.Register(() =>
            {
                dispatcher.Wait();
                dispatcher.WaitAll();
            });
        }
    }

}
