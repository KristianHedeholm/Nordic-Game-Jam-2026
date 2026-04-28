using System;
using System.Text;
using RawPowerLabs.DynamicAI.Native;

namespace RawPowerLabs.DynamicAI
{
    public unsafe class TextModuleInput : ResourceHandle
    {
        private readonly Context _context;

        internal TextModuleInput(Context context, int handle)
        {
            _context = context;
            SetHandle((IntPtr)handle);
        }

        public bool Set(string key, int value)
        {
            fixed (byte* pKey = NativeUtils.EncodeUtf8(key))
            {
                return CoreInterface.Instance->TextModuleInput.SetInt((int)_context.DangerousGetHandle(), (int)handle, pKey, value);
            }
        }

        public bool GetInteger(string key, ref int value)
        {
            fixed (byte* pKey = NativeUtils.EncodeUtf8(key))
            {
                return CoreInterface.Instance->TextModuleInput.GetInt((int)_context.DangerousGetHandle(), (int)handle, pKey, ref value);
            }
        }

        public bool Set(string key, string value)
        {
            var valueBytes = NativeUtils.EncodeUtf8(value);

            fixed (byte* pKey    = NativeUtils.EncodeUtf8(key))
            fixed (byte* pBuffer = valueBytes)
            {
                return CoreInterface.Instance->TextModuleInput.SetString(
                    (int)_context.DangerousGetHandle(), (int)handle, pKey, pBuffer, valueBytes.Length - 1);
            }
        }

        public string? GetString(string key)
        {
            fixed (byte* pKey = NativeUtils.EncodeUtf8(key))
            {
                nint bufferSize = 0;
                if (!CoreInterface.Instance->TextModuleInput.GetString((int)_context.DangerousGetHandle(), (int)handle, pKey, null, ref bufferSize))
                    return null;

                var buffer = new byte[bufferSize];

                fixed (byte* pBuffer = buffer)
                {
                    if (!CoreInterface.Instance->TextModuleInput.GetString((int)_context.DangerousGetHandle(), (int)handle, pKey, pBuffer, ref bufferSize))
                        return null;

                    return Encoding.UTF8.GetString(pBuffer, (int)bufferSize);
                }
            }
        }

        protected override void Destroy()
        {
            CoreInterface.Instance->TextModuleInput.Destroy((int)_context.DangerousGetHandle(), (int)handle);
        }
    }
}
