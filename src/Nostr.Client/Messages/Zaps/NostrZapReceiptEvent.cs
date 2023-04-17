using BTCPayServer.Lightning;
using NBitcoin;
using Newtonsoft.Json;
using Nostr.Client.Json;

namespace Nostr.Client.Messages.Zaps
{
    public class NostrZapReceiptEvent : NostrEvent
    {
        /// <summary>
        /// Hex-encoded pubkey of the recipient
        /// </summary>
        [JsonIgnore]
        public string? RecipientPubkey => Tags?.FindFirstTagValue("p");

        /// <summary>
        /// Optional hex-encoded event id. Clients MUST include this if zapping an event rather than a person.
        /// </summary>
        [JsonIgnore]
        public string? EventId => Tags?.FindFirstTagValue("e");

        [JsonIgnore]
        public string? Bolt11 => Tags?.FindFirstTagValue("bolt11");

        [JsonIgnore]
        public NostrEvent? Description => NostrJson.DeserializeSafely<NostrEvent>(Tags?.FindFirstTagValue("description"));

        [JsonIgnore]
        public string? Preimage => Tags?.FindFirstTagValue("preimage");

        public BOLT11PaymentRequest? DecodeBolt11()
        {
            if (string.IsNullOrWhiteSpace(Bolt11))
                return null;

            return BOLT11PaymentRequest.TryParse(Bolt11, out var request, Network.Main) ?
                request : null;
        }
    }
}
