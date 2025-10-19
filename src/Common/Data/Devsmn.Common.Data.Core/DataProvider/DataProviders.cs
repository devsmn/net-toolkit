using Devsmn.Common.Diagnostics;
using Devsmn.Common.Service.Core;
using System.Diagnostics;

namespace Devsmn.Common.Data.Core.DataProvider
{
    public static class DataProviders
    {
        private static readonly Dictionary<Type, IRepository> Stores = new();

        /// <summary>
        /// Registers the given <paramref name="repository"/>.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <param name="context"></param>
        /// <param name="repository"></param>
        public static void Register<TRepository>(IContext context, TRepository repository)
            where TRepository : class, IRepository
        {
            try {
                Stores.Add(repository.GetType(), repository);
            }
            catch (Exception ex) {
                context.Log(ex);
            }
        }

        /// <summary>
        /// Closes all data stores.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task CloseAllAsync(IContext context)
        {
            if (Stores.Count == 0)
                return;

            foreach (IRepository repository in Stores.Values) {
                try {
                    await repository.CloseAsync();
                }
                catch (Exception ex) {
                    context.Log("Unable to close repository: ");
                    context.Log(ex);
                }
            }
        }

        /// <summary>
        /// Clears the data stores.
        /// </summary>
        public static void Clear()
        {
            Stores.Clear();
        }

        /// <summary>
        /// Initializes the registered repositories.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compatService"></param>
        /// <param name="loginFailed"></param>
        /// <returns></returns>
        public static async Task<bool> InitializeAsync(
            IContext context,
            ICompatibilityService compatService,
            Action loginFailed)
        {
            foreach (IRepository instance in Stores.Values) {
                try {
                    await instance.InitializeAsync(context);
                    instance.RegisterPatches(context, compatService);
                    await instance.ExecutePatches(context, compatService);
                }
                catch (Exception ex) {
                    context.Log(ex);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves the instance for the given repository.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        internal static TRepository Resolve<TRepository>()
            where TRepository : IRepository
        {
            foreach (KeyValuePair<Type, IRepository> store in Stores) {
                if (store.Value is TRepository value)
                    return value;
            }

            return default;
        }
    }
}


