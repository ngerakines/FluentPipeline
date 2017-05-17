namespace FluentPipeline.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using FluentPipeline.Core;
    using Microsoft.Extensions.Logging;

    public static class DispatcherServiceExtensions
    {

        public static void UseDispatcher(this IApplicationBuilder builder, ILoggerFactory loggerFactory)
        {
            var lifetime = (IApplicationLifetime)builder.ApplicationServices.GetService(typeof(IApplicationLifetime));

            var stoppingToken = lifetime.ApplicationStopping;
            var stoppedToken = lifetime.ApplicationStopped;

            var dispatcherFactory = new DispatcherFactory(loggerFactory);
            var dispatcher = dispatcherFactory.Create();

            dispatcher.Run();
            dispatcher.StartWorker(loggerFactory);

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
