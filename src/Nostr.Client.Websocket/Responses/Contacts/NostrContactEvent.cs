using Nostr.Client.Websocket.Json;

namespace Nostr.Client.Websocket.Responses.Contacts
{
    public class NostrContactEvent : NostrEvent
    {
        public NostrContactEvent(string? content)
        {
            Content = content;
            Relays = NostrSerializer.DeserializeSafely<NostrRelays>(content) ??
                     new NostrRelays(new Dictionary<string, NostrRelayConfig>());
        }

        public NostrRelays Relays { get; init; }
    }
}
