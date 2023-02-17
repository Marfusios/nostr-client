using System.Diagnostics;
using Nostr.Client.Json;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("{MessageType} - {Message}")]
    public class NostrNoticeResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Message { get; init; }
    }
}
