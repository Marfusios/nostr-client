using System.Diagnostics;
using Nostr.Client.Websocket.Json;

namespace Nostr.Client.Websocket.Responses
{
    [DebuggerDisplay("{MessageType} - {Subscription}")]
    public class NostrEoseResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Subscription { get; init; }
    }
}
