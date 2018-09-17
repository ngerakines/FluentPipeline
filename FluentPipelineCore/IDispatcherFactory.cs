namespace FluentPipeline.Core
{
    public interface IDispatcherFactory<T>
    {
        IDispatcher<T> Create();
    }
}