using Newtonsoft.Json;
using Nostr.Client.Json;
using Nostr.Client.Messages;

namespace Nostr.Client.Requests
{
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrEventRequest
    {
        // for deserialization in tests
        private NostrEventRequest()
        {
            Event = null!;
        }

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
