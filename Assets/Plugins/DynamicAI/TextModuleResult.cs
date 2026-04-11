using System;
using System.Text;
using RawPowerLabs.DynamicAI.Native;

namespace RawPowerLabs.DynamicAI
{
    public unsafe class TextModuleResult : ResourceHandle
    {
        private readonly Context _context;

        internal TextModuleResult(Context context, int handle)
        {
            _context = context;
            SetHandle((IntPtr)handle);
        }

        public bool GetInteger(string key, ref int value)
        {
            fixed (byte* pKey = NativeUtils.EncodeUtf8(key))
            {
                return CoreInterface.Instance->TextModuleResult.GetInt((int)_context.DangerousGetHandle(), (int)handle, pKey, ref value);
            }
        }

        public string? GetString(string key)
        {
            fixed (byte* pKey = NativeUtils.EncodeUtf8(key))
            {
                nint bufferSize = 0;
                if (!CoreInterface.Instance->TextModuleResult.GetString((int)_context.DangerousGetHandle(), (int)handle, pKey, null, ref bufferSize))
                    return null;

                var buffer = new byte[bufferSize];

                fixed (byte* pBuffer = buffer)
                {
                    if (!CoreInterface.Instance->TextModuleResult.GetString((int)_context.DangerousGetHandle(), (int)handle, pKey, pBuffer, ref bufferSize))
                        return null;

                    return Encoding.UTF8.GetString(pBuffer, (int)bufferSize);
                }
            }
        }

        protected override void Destroy()
        {
            CoreInterface.Instance->TextModuleResult.Destroy((int)_context.DangerousGetHandle(), (int)handle);
        }
    }
}
