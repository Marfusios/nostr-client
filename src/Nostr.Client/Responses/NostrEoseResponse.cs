using System.Diagnostics;
using Nostr.Client.Json;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("[{CommunicatorName}] {MessageType} - {Subscription}")]
    public class NostrEoseResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Subscription { get; init; }
    }
}
