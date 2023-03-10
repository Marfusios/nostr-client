using System.Security.Cryptography;
using System.Text;

namespace Nostr.Client.Utils
{
    public static class HashExtensions
    {
        /// <summary>
        /// Compute SHA256 hash from the given input
        /// </summary>
        public static byte[] GetSha256(string data)
        {
            var sha256 = SHA256.HashData(FromString(data));
            return sha256;
        }

        /// <summary>
        /// Get bytes from the string (UTF8)
        /// </summary>
        public static byte[] FromString(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Get string (UTF8) from bytes
        /// </summary>
        public static string ToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
