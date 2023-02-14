using System.Diagnostics;
using Newtonsoft.Json;
using Nostr.Client.Websocket.Json;

namespace Nostr.Client.Websocket.Responses
{
    [DebuggerDisplay("{MessageType} - {Subscription}")]
    public class NostrEventResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Subscription { get; init; }

        [ArrayProperty(2)]
        [JsonConverter(typeof(NostrEventConverter))]
        public NostrEvent? Event { get; init; }
    }
}
