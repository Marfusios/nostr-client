using Newtonsoft.Json;
using Nostr.Client.Json;
using Nostr.Client.Messages;

namespace Nostr.Client.Requests
{
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrRequest
    {
        public NostrRequest(string subscription, NostrFilter nostrFilter)
        {
            Subscription = subscription;
            NostrFilter = nostrFilter;
        }

        [ArrayProperty(0)]
        public string Type { get; init; } = NostrMessageTypes.Request;

        [ArrayProperty(1)]
        public string Subscription { get; init; }

        [ArrayProperty(2)]
        public NostrFilter NostrFilter { get; init; }
    }
}
