using Newtonsoft.Json;
using Nostr.Client.Json;
using Nostr.Client.Messages;

namespace Nostr.Client.Requests
{
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrCloseRequest
    {
        public NostrCloseRequest(string subscription)
        {
            Subscription = subscription;
        }

        [ArrayProperty(0)]
        public string Type { get; init; } = NostrMessageTypes.Close;

        [ArrayProperty(1)]
        public string Subscription { get; init; }
    }
}
