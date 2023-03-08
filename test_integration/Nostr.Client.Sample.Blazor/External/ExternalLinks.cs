using Nostr.Client.Utils;

namespace Nostr.Client.Sample.Blazor.External
{
    public class ExternalLinks
    {
        public static string GetLinkToProfile(string? key)
        {
            return $"https://snort.social/p/{FormatToNpub(key)}";
        }

        public static string GetLinkToEvent(string? key)
        {
            return $"https://snort.social/e/{FormatToNote(key)}";
        }

        public static string? FormatToNpub(string? hexKey) => FormatToBech32(hexKey, "npub");
        public static string? FormatToNote(string? hexKey) => FormatToBech32(hexKey, "note");
        public static string? FormatToBech32(string? hexKey, string hrp)
        {
            try
            {
                return NostrConverter.ToBech32(hexKey, hrp);
            }
            catch (Exception)
            {
                return hexKey;
            }
        }
    }
}
