namespace Nostr.Client.Websocket.Utils
{
    public static class NostrConverter
    {
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

        public static string? ToBech32(string? hexKey, string hrp)
        {
            if (string.IsNullOrWhiteSpace(hexKey))
                return hexKey;

            var hexArray = HexExtensions.ToByteArray(hexKey);
            var npub = Bech32.Encode(hrp, hexArray);
            return npub;
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
    }
}
