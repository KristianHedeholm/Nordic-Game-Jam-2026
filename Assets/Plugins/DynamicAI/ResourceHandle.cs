using System.Runtime.InteropServices;

namespace RawPowerLabs.DynamicAI
{
    public abstract class ResourceHandle : SafeHandle
    {
        protected ResourceHandle()
            : base((System.IntPtr)InvalidHandle, true)
        {
        }

        public const int InvalidHandle = -1;

        public override bool IsInvalid => (int)handle == InvalidHandle;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid) return true;

            Destroy();

            // Clear the handle to avoid double releases.
            SetHandle((System.IntPtr)InvalidHandle);

            return true;
        }

        protected abstract void Destroy();
    }
}
