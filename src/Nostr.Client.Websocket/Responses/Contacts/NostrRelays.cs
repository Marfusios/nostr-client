using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Nostr.Client.Websocket.Responses.Contacts
{
    [DebuggerDisplay("Relays {Count}")]
    public class NostrRelays : ReadOnlyDictionary<string, NostrRelayConfig>
    {
        public NostrRelays(IDictionary<string, NostrRelayConfig> dictionary) : base(dictionary)
        {
        }
    }
}
