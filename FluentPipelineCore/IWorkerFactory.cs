namespace FluentPipeline.Core
{
    using System.Collections.Concurrent;

    public interface IWorkerFactory<T>
    {
        IWorker Create(IProducerConsumerCollection<T> workQueue);
    }
}