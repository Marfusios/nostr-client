using Newtonsoft.Json;
using Nostr.Client.Json;
using Nostr.Client.Messages;
using Nostr.Client.Responses;

namespace Nostr.Client.Requests
{
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrEventRequest
    {
        public NostrEventRequest(NostrEvent eventData)
        {
            Event = eventData;
        }

        [ArrayProperty(0)]
        public string Type { get; init; } = NostrMessageTypes.Event;

        [ArrayProperty(1)]
        public NostrEvent Event { get; init; }
    }
}
