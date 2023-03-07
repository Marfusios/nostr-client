using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Nostr.Client.Messages.Contacts
{
    [DebuggerDisplay("Relays {Count}")]
    public class NostrRelays : ReadOnlyDictionary<string, NostrRelayConfig>
    {
        public NostrRelays(IDictionary<string, NostrRelayConfig> dictionary) : base(dictionary)
        {
        }
    }
}
