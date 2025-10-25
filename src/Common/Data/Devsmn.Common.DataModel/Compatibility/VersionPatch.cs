using Devsmn.Common.Diagnostics;

namespace Devsmn.Common.DataModel
{
    public class VersionPatch
    {
        private class PatchInstance
        {
            private readonly Func<IContext, Task<bool>> _func;
            private bool _successful;

            public PatchInstance(Func<IContext, Task<bool>> func)
            {
                _func = func;
            }

            public async Task Patch(IContext context)
            {
                if (_successful)
                    return;

                _successful = await _func(context);
            }
        }

        public int Version { get; }

        private readonly List<PatchInstance> _patches;

        /// <summary>
        /// Initializes a new instance of <see cref="VersionPatch"/>.
        /// </summary>
        /// <param name="version">The version this patch is applied to</param>
        /// <param name="patches">The patches to be applied</param>
        public VersionPatch(int version, params Func<IContext, Task<bool>>[] patches)
        {
            Version = version;
            _patches = new();
            _patches.AddRange(patches.Select(x => new PatchInstance(x)));
        }

        public void AddPatch(Func<IContext, Task<bool>> action)
        {
            _patches.Add(new PatchInstance(action));
        }

        /// <summary>
        /// Executes the patches.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task PatchAsync(IContext context)
        {
            foreach (PatchInstance patch in _patches)
                await patch.Patch(context);
        }
    }
}
