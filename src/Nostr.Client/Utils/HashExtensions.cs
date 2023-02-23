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

        public static byte[] FromString(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
