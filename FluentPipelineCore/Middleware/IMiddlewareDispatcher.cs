namespace FluentPipeline.Core.Middleware
{
    public interface IMiddlewareDispatcher
    {
        void AddMiddleware(IMiddleware middleware);

        void Dispatch(string input);
    }

}
