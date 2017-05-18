namespace FluentPipeline.Core
{
    public class DefaultBackoffPolicy : StaticBackoffPolicy
    {
        public DefaultBackoffPolicy() : base(5000)
        {
        }
    }
}
