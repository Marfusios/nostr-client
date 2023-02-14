using System.Diagnostics;
using Nostr.Client.Websocket.Json;

namespace Nostr.Client.Websocket.Responses
{
    [DebuggerDisplay("{MessageType} - {Message}")]
    public class NostrNoticeResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Message { get; init; }
    }
}
