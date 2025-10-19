using Devsmn.Common.Diagnostics;
using Devsmn.Common.Service.Core;

namespace Devsmn.Common.Data.Core.DataProvider
{
    /// <summary>
    /// <see cref="IDataProxy"/> provides common functionality to retrieve data stores.
    /// </summary>
    public interface IDataProxy
    {
        /// <summary>
        /// Requests an instance of a <see cref="IRepository"/> of the given <typeparamref name="TRepository"/>.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        TRepository Request<TRepository>(IDataProxyParameter parameter)
            where TRepository : class, IRepository;

        /// <summary>
        /// Requests the <see cref="IDataProviderPatcher"/> for this <see cref="IDataProxy"/>.
        /// </summary>
        /// <returns></returns>
        IDataProviderPatcher RequestPatcher();

        /// <summary>
        /// Requests the <see cref="IDataProviderAuthenticator"/> for this <see cref="IDataProxy"/>.
        /// </summary>
        /// <returns></returns>
        IDataProviderAuthenticator RequestAuthenticator();
    }
}
