using System.Text;

namespace Nostr.Client.Utils
{
    public static class HexExtensions
    {
        public static byte[] ToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            var arr = new byte[hex.Length >> 1];

            for (var i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexValue(hex[i << 1]) << 4) + (GetHexValue(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static bool IsHex(string hex)
        {
            if (hex.Length % 2 == 1)
                return false;
            foreach(var c in hex.ToArray())
            {
                var isHex = (c >= '0' && c <= '9') || 
                            (c >= 'a' && c <= 'f') || 
                            (c >= 'A' && c <= 'F');

                if(!isHex)
                    return false;
            }
            return true;
        }
        
        public static int GetHexValue(char hex)
        {
            var val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string ToHex(this byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var builder = new StringBuilder();
            foreach (var t in bytes)
            {
                builder.Append(t.ToHex());
            }

            return builder.ToString();
        }

        public static string ToHex(this Span<byte> bytes)
        {
            var builder = new StringBuilder();
            foreach (var t in bytes)
            {
                builder.Append(t.ToHex());
            }

            return builder.ToString();
        }

        private static string ToHex(this byte b)
        {
            return b.ToString("x2");
        }
    }
}
