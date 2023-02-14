using Nostr.Client.Websocket.Json;
using Nostr.Client.Websocket.Messages;

namespace Nostr.Client.Websocket.Requests
{
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
