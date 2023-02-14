using Newtonsoft.Json;
using Nostr.Client.Websocket.Messages;

namespace Nostr.Client.Websocket.Requests
{
    public class NostrFilter
    {
        /// <summary>
        /// A list of event ids or prefixes
        /// </summary>
        public string[]? Ids { get; init; }

        /// <summary>
        /// A list of pubkeys or prefixes, the pubkey of an event must be one of these
        /// </summary>
        public string[]? Authors { get; init; }

        /// <summary>
        /// A list of a kind numbers
        /// </summary>
        public NostrKind[]? Kinds { get; init; }

        /// <summary>
        /// A list of event ids that are referenced in an "e" tag
        /// </summary>
        [JsonProperty("#e")]
        public string[]? E { get; init; }

        /// <summary>
        /// A list of pubkeys that are referenced in a "p" tag
        /// </summary>
        [JsonProperty("#p")]
        public string[]? P { get; init; }

        /// <summary>
        /// Events must be newer than this to pass
        /// </summary>
        public DateTime? Since { get; init; }

        /// <summary>
        /// Events must be older than this to pass
        /// </summary>
        public DateTime? Until { get; init; }

        /// <summary>
        /// Maximum number of events to be returned in the initial query
        /// </summary>
        public int? Limit { get; init; }
    }
}
