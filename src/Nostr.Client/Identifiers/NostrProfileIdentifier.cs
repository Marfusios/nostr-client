using System.Text;
using Nostr.Client.Utils;

namespace Nostr.Client.Identifiers
{
    public class NostrProfileIdentifier : NostrIdentifier
    {
        public NostrProfileIdentifier(string pubkey, string[]? relays)
        {
            Pubkey = pubkey;
            Relays = relays ?? Array.Empty<string>();
        }

        public override string Hrp => "nprofile";
        public override string Special => Pubkey;
        public string Pubkey { get; init; }
        public string[] Relays { get; init; }

        public override string ToBech32()
        {
            var tlvData = new List<(byte, byte[])>
            {
                (SpecialKey, Convert.FromHexString(Pubkey)),
            };
            tlvData.AddRange(Relays.Select(relay => (RelayKey, Encoding.ASCII.GetBytes(relay))));
            var tlv = NostrIdentifierParser.BuildTlv(tlvData.ToArray());
            return NostrConverter.ToBech32(tlv, Hrp) ?? string.Empty;
        }

        public static NostrProfileIdentifier Parse(byte[] data)
        {
            var tlv = NostrIdentifierParser.ParseTlv(data);
            return new NostrProfileIdentifier(
                FindSpecialHex(tlv),
                FindRelays(tlv)
            );
        }
    }
}
