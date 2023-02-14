using System.Diagnostics;
using Nostr.Client.Websocket.Json;

namespace Nostr.Client.Websocket.Responses
{
    [DebuggerDisplay("{MessageType} - {Subscription}")]
    public class NostrEventResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Subscription { get; init; }

        [ArrayProperty(2)]
        public NostrEvent? Event { get; init; }
    }
}
