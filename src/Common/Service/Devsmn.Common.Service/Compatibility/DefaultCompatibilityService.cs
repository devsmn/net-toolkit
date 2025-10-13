using Devsmn.Common.DataModel;
using Devsmn.Common.Diagnostics;
using Devsmn.Common.Service.Core;

namespace Devsmn.Common.Service
{
    /// <summary>
    /// The default implementation of the <see cref="ICompatibilityService"/>.
    /// </summary>
    public abstract class DefaultCompatibilityService : ICompatibilityService
    {
        private readonly Dictionary<Type, List<VersionPatch>?> _patches;

        protected DefaultCompatibilityService()
        {
            _patches = new();
        }

        public IEnumerable<VersionPatch> GetPatches<TFor>(IContext context)
        {
            if (!_patches.TryGetValue(typeof(TFor), out List<VersionPatch>? list)) {
                context.Log($"No patches available for type=[{typeof(TFor).FullName}]");
                yield break;
            }

            if (list == null)
                yield break;

            int fromVersion = GetLastUsedVersion();

            foreach (VersionPatch patch in list)
            {
                if (fromVersion < patch.Version)
                {
                    context.Log($"Patch with version=[{patch.Version}] found, fromVersion=[{fromVersion}] for type=[{typeof(TFor).FullName}]");
                    yield return patch;
                }
            }
        }

        public void RegisterPatch<TFor>(VersionPatch patch)
        {
            if (!_patches.ContainsKey(typeof(TFor)))
                _patches.Add(typeof(TFor), new List<VersionPatch>());

            _patches[typeof(TFor)]?.Add(patch);
        }

        public abstract int GetLastUsedVersion();
        public abstract int GetCurrentVersion();
        public abstract void UpdateLastUsedVersion(IContext context);
    }
}
