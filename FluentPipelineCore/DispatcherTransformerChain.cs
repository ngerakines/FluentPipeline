namespace FluentPipeline.Core
{
    using System;
    using System.Collections.Generic;

    public interface IDispatcherTransformerChain<T>
    {
        IDispatcherTransformerChain<T> Into(IDispatcherTransformer<T> transformer);

        T Transform(T input);
    }

    public interface IDispatcherTransformer<T>
    {
        T Transform(T input);
    }

    public class DefaultDispatcherTransformerChain<T> : IDispatcherTransformerChain<T>
    {
        public IDispatcherTransformerChain<T> Into(IDispatcherTransformer<T> transformer)
        {
            throw new NotImplementedException();
        }

        public T Transform(T input)
        {
            return input;
        }
    }

    public class DispatcherTransformerChain<T> : IDispatcherTransformerChain<T>
    {
        private IList<IDispatcherTransformer<T>> transformers;

        public DispatcherTransformerChain()
        {
            transformers = new List<IDispatcherTransformer<T>>();
        }

        public IDispatcherTransformerChain<T> Into(IDispatcherTransformer<T> transformer)
        {
            transformers.Add(transformer);
            return this;
        }

        public T Transform(T input)
        {
            var output = input;
            foreach (var transformer in transformers)
            {
                output = transformer.Transform(output);
            }
            return output;
        }
    }

    public class SNSTransformer : IDispatcherTransformer<string>
    {
        public string Transform(string input)
        {
            throw new NotImplementedException();
        }
    }

}