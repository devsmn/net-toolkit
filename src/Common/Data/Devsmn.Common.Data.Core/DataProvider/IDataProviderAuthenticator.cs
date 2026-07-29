namespace Devsmn.Common.Data.Core.DataProvider
{
    public interface IDataProviderAuthenticator
    {
        /// <summary>
        /// Authenticates the data provider with the given <paramref name="passkey"/>.
        /// </summary>
        /// <param name="passkey"></param>
        /// <returns></returns>
        Task<bool> AuthenticateAsync(string passkey);

        /// <summary>
        /// Authenticates the data provider at the given location with the provided <paramref name="passkey"/>.
        /// </summary>
        /// <param name="passkey"></param>
        /// <param name="localPath"></param>
        /// <returns></returns>
        Task<bool> AuthenticateAsync(string passkey, string localPath);
    }
}
