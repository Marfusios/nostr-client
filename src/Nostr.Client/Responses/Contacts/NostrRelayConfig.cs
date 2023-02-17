using System.Diagnostics;
using Newtonsoft.Json;

namespace Nostr.Client.Responses.Contacts
{
    [DebuggerDisplay("Relay, read: {Read}, write: {Write}")]
    public class NostrRelayConfig
    {
        /// <summary>
        /// Relay is enabled for loading data from
        /// </summary>
        public bool Read { get; init; }

        /// <summary>
        /// Relay is enabled for sending data to
        /// </summary>
        public bool Write { get; init; }

        /// <summary>
        /// Additional unparsed data
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; init; } = new();
    }
}
