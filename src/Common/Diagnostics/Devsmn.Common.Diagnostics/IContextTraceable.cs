namespace Devsmn.Common.Diagnostics
{
    public interface IContextTraceable
    {
        Guid CorrelationId
        {
            get;
        }

        int ThreadId
        {
            get;
        }
    }
}
