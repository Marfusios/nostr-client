namespace Nostr.Client.Utils
{
    /// <summary>
    /// Nostr conversion utilities
    /// </summary>
    public static class NostrConverter
    {
        /// <summary>
        /// Convert Bech32 string into hex byte array key
        /// </summary>
        public static byte[]? ToHexBytes(string? bech32, out string? hrp)
        {
            hrp = null;
            if (string.IsNullOrWhiteSpace(bech32))
                return Array.Empty<byte>();

            Bech32.Decode(bech32, out hrp, out var decoded);
            return decoded;
        }

        /// <summary>
        /// Convert Bech32 string into hex key
        /// </summary>
        public static string? ToHex(string? bech32, out string? hrp)
        {
            hrp = null;
            if (string.IsNullOrWhiteSpace(bech32))
                return bech32;

            Bech32.Decode(bech32, out hrp, out var decoded);
            var hex = decoded?.ToHex();
            return hex;
        }

        /// <summary>
        /// Try to convert Bech32 string into hex key
        /// </summary>
        public static bool TryToHex(string? bech32, out string? hex, out string? hrp)
        {
            hrp = null;
            hex = null;
            try
            {
                hex = ToHex(bech32, out hrp);
                return !string.IsNullOrWhiteSpace(hex);
            }
            catch (Exception)
            {
                // ignore
                return false;
            }
        }

        /// <summary>
        /// Convert hex string to Bech32 format, you need to provide hrp (prefix)
        /// </summary>
        public static string? ToBech32(string? hexKey, string hrp)
        {
            if (string.IsNullOrWhiteSpace(hexKey))
                return hexKey;

            var hexArray = HexExtensions.ToByteArray(hexKey);
            return ToBech32(hexArray, hrp);
        }

        /// <summary>
        /// Convert hex byte array to Bech32 format, you need to provide hrp (prefix)
        /// </summary>
        public static string? ToBech32(byte[]? hexArray, string hrp)
        {
            if (hexArray == null)
                return null;

            var npub = Bech32.Encode(hrp, hexArray);
            return npub;
        }

        /// <summary>
        /// Try to convert hex string to Bech32 format, you need to provide hrp (prefix)
        /// </summary>
        public static bool TryToBech32(string? hexKey, string hrp, out string? bech32)
        {
            bech32 = null;
            try
            {
                bech32 = ToBech32(bech32, hrp);
                return !string.IsNullOrWhiteSpace(bech32);
            }
            catch (Exception)
            {
                // ignore
                return false;
            }
        }

        /// <summary>
        /// Convert hex key into Bech32 'npub1xxx' representation
        /// </summary>
        public static string? ToNpub(string? hexKey)
        {
            return ToBech32(hexKey, "npub");
        }

        /// <summary>
        /// Convert hex key into Bech32 'nsec1xxx' representation
        /// </summary>
        public static string? ToNsec(string? hexKey)
        {
            return ToBech32(hexKey, "nsec");
        }

        /// <summary>
        /// Convert hex key into Bech32 'note1xxx' representation
        /// </summary>
        public static string? ToNote(string? hexKey)
        {
            return ToBech32(hexKey, "note");
        }
    }
}
