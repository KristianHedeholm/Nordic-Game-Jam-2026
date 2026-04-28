using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RawPowerLabs.DynamicAI
{
    public static unsafe class CoreDiagnostics
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LogCallbackDelegate(LogLevel level, IntPtr text, IntPtr size);

        // Keep a static reference to prevent garbage collection
        private static readonly LogCallbackDelegate logCallback = OnLog;

        // Get the function pointer to pass to native code
        public static IntPtr LogCallbackPointer => Marshal.GetFunctionPointerForDelegate(logCallback);

        internal static void OnLog(LogLevel level, IntPtr text, IntPtr size)
        {
            Log?.Invoke(new LogEvent(level, Encoding.UTF8.GetString((byte*)text, (int)size)));
        }

        public static event Action<LogEvent>? Log;
    }
}
