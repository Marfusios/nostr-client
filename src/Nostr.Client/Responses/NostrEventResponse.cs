using System.Diagnostics;
using Newtonsoft.Json;
using Nostr.Client.Json;
using Nostr.Client.Messages;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("[{CommunicatorName}] {MessageType} - {Subscription}")]
    public class NostrEventResponse : NostrResponse
    {
        [ArrayProperty(1)]
        public string? Subscription { get; init; }

        [ArrayProperty(2)]
        [JsonConverter(typeof(NostrEventConverter))]
        public NostrEvent? Event { get; init; }
    }
}
