using Devsmn.Common.Diagnostics;
using Devsmn.Common.Service.Core;

namespace Devsmn.Common.Data.Core
{
    /// <summary>
    /// Provides common
    /// </summary>
    public interface IRepository
    {
        bool IsValid { get; }

        /// <summary>
        /// Determines whether the repository can be resolved based on the given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        bool CanResolveFor(DataProviderRequestParameter? parameter);

        /// <summary>
        /// Asynchronously initializes the repository.
        /// <para>
        /// This can be used to initialize required resources.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> InitializeAsync(IContext context);

        /// <summary>
        /// Registers patches to maintain compatibility. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compatService"></param>
        void RegisterPatches(IContext context, ICompatibilityService compatService);

        /// <summary>
        /// Executes registered patches (<see cref="RegisterPatches"/>).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compatService"></param>
        /// <returns></returns>
        Task ExecutePatches(IContext context, ICompatibilityService compatService);

        /// <summary>
        /// Closes the repository.
        /// </summary>
        Task CloseAsync();
    }
}
