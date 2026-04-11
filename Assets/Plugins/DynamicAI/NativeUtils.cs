using System.Text;

namespace RawPowerLabs.DynamicAI
{
    internal static class NativeUtils
    {
        /// <summary>
        /// Encodes a string as a null-terminated UTF-8 byte array suitable for passing to native code via <c>fixed</c>.
        /// </summary>
        internal static byte[] EncodeUtf8(string value)
        {
            var bytes = new byte[Encoding.UTF8.GetByteCount(value) + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 0);
            return bytes; // last byte is already 0
        }
    }
}
