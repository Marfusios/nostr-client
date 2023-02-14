using System.Diagnostics;
using Newtonsoft.Json;
using Nostr.Client.Websocket.Messages;

namespace Nostr.Client.Websocket.Responses
{
    [DebuggerDisplay("{CreatedAt} {Kind.ToString()}")]
    public class NostrEvent
    {
        /// <summary>
        /// 32-bytes lowercase hex-encoded sha256 of the the serialized event data
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// 32-bytes lowercase hex-encoded public key of the event creator
        /// </summary>
        public string? Pubkey { get; init; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; init; }

        public NostrKind Kind { get; init; }

        public NostrEventTag[]? Tags { get; init; }

        /// <summary>
        /// Arbitrary string
        /// </summary>
        public string? Content { get; init; }

        /// <summary>
        /// 64-bytes hex of the signature of the sha256 hash of the serialized event data, which is the same as the "id" field
        /// </summary>
        public string? Sig { get; init; }

        /// <summary>
        /// Additional unparsed
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; init; } = new();
    }
}
