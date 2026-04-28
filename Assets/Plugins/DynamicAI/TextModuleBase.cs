using System;
using RawPowerLabs.DynamicAI.Native;

namespace RawPowerLabs.DynamicAI
{
    public abstract unsafe class TextModuleBase : ResourceHandle, ITextModule
    {
        private readonly Context _context;

        protected TextModuleBase(Context context, int handle)
        {
            _context = context;
            SetHandle((IntPtr)handle);
        }

        public TextModuleInput CreateInput()
        {
            return new TextModuleInput(_context, CoreInterface.Instance->TextModuleInput.Create((int)_context.DangerousGetHandle(), (int)handle));
        }

        public TextModuleResult? Invoke(TextModuleInvokeParameters parameters, TextModuleInput input)
        {
            var result = CoreInterface.Instance->TextModule.Invoke((int)_context.DangerousGetHandle(), (int)handle, ref parameters, (int)input.DangerousGetHandle());
            return result == InvalidHandle ? null : new TextModuleResult(_context, result);
        }

        protected override void Destroy()
        {
            CoreInterface.Instance->TextModule.Destroy((int)_context.DangerousGetHandle(), (int)handle);
        }
    }
}
