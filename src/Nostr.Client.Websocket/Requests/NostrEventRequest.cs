using Newtonsoft.Json;
using Nostr.Client.Websocket.Json;
using Nostr.Client.Websocket.Messages;
using Nostr.Client.Websocket.Responses;

namespace Nostr.Client.Websocket.Requests
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
