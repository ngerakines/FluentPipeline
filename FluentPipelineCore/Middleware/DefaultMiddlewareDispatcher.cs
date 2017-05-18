namespace FluentPipeline.Core.Middleware
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DefaultMiddlewareDispatcher : IMiddlewareDispatcher
    {
        private readonly IList<IMiddleware> middleware;

        public DefaultMiddlewareDispatcher()
        {
            middleware = new List<IMiddleware>();
        }

        public DefaultMiddlewareDispatcher(IEnumerable<IMiddleware> middleware)
        {
            this.middleware = new List<IMiddleware>();
            foreach (var m in middleware)
            {
                this.middleware.Add(m);
            }
        }

        public void AddMiddleware(IMiddleware middleware)
        {
            this.middleware.Add(middleware);
        }

        public void Dispatch(string input)
        {
            var parsedInput = JObject.Parse(input);
            Task.WaitAll(middleware.Where(m => m.CanHandle(parsedInput)).Select(m => m.Handle(parsedInput)).ToArray());
        }
    }

}
