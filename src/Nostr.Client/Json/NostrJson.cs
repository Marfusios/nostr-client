using Newtonsoft.Json;

namespace Nostr.Client.Json
{
    /// <summary>
    /// Nostr JSON serializer and deserializer
    /// </summary>
    public static class NostrJson
    {
        /// <summary>
        /// Serialize Nostr type into json, use preconfigured serializer
        /// </summary>
        public static string? Serialize<TNostrType>(TNostrType? obj)
        {
            if (obj == null)
                return null;
            return JsonConvert.SerializeObject(obj, NostrSerializer.Settings);
        }

        /// <summary>
        /// Deserialize json into Nostr type, use preconfigured serializer
        /// </summary>
        public static TNostrType? Deserialize<TNostrType>(string? json) where TNostrType : class
        {
            if (json == null)
                return null;
            return JsonConvert.DeserializeObject<TNostrType>(json, NostrSerializer.Settings);
        }
    }
}
