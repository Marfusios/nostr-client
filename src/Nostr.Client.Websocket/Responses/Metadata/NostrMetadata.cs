using Newtonsoft.Json;

namespace Nostr.Client.Websocket.Responses.Metadata
{
    public class NostrMetadata
    {
        public string? Name { get; init; }

        public string? About { get; init; }

        public string? Picture { get; init; }

        public string? Nip05 { get; init; }

        public string? Lud16 { get; init; }

        public string? Banner { get; init; }

        public string? Nip57 { get; init; }

        /// <summary>
        /// Additional unparsed data
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; init; } = new();
    }
}
