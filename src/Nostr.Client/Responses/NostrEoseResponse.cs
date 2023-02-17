using System.Diagnostics;
using Nostr.Client.Json;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("{MessageType} - {Subscription}")]
    public class NostrEoseResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Subscription { get; init; }
    }
}
