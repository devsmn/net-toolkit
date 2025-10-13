namespace Devsmn.Common.Diagnostics
{
    public interface IStateAware
    {
        CancellationToken CancellationToken
        {
            get;
        }
    }
}
