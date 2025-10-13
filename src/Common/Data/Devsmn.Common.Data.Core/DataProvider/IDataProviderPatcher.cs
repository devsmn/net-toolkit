using Devsmn.Common.Diagnostics;
using Devsmn.Common.Service.Core;

namespace Devsmn.Common.Data.Core.DataProvider
{
    public interface IDataProviderPatcher
    {
        /// <summary>
        /// Registers the patches for a service provider.
        /// </summary>
        /// <param name="service"></param>
        void RegisterPatches(ICompatibilityService service);

        /// <summary>
        /// Executes the patches for a service provider.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        Task ExecutePatchesAsync(IContext context, ICompatibilityService service);
    }
}
