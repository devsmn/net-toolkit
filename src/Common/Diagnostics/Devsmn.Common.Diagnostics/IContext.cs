namespace Devsmn.Common.Diagnostics
{
    public interface IContext : IStateAware, IContextTraceable
    {
        void Log(string message);
        void Log(Exception exception);
        void Log(AggregateException aggregateException);
    }
}
