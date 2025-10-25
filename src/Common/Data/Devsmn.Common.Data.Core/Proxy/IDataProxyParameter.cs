namespace Devsmn.Common.Data.Core.DataProvider
{
    /// <summary>
    /// <see cref="IDataProxyParameter"/> is the base interface for defining proxy parameters 
    /// that should be considered when retrieving data repositories.
    /// </summary>
    public interface IDataProxyParameter
    {
        Version MinimumVersion
        {
            get; set;
        }
    }
}
