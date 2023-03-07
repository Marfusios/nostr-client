using Newtonsoft.Json;
using Nostr.Client.Json;

namespace Nostr.Client.Messages.Contacts
{
    public class NostrContactEvent : NostrEvent
    {
        public NostrContactEvent(string? content)
        {
            Content = content;
            Relays = NostrSerializer.DeserializeSafely<NostrRelays>(content) ??
                     new NostrRelays(new Dictionary<string, NostrRelayConfig>());
        }

        [JsonIgnore]
        public NostrRelays Relays { get; init; }
    }
}
