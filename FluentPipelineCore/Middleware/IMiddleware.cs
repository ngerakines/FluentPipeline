namespace FluentPipeline.Core.Middleware
{
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    public interface IMiddleware
    {
        bool CanHandle(JObject o);

        Task Handle(JObject o);
    }

}