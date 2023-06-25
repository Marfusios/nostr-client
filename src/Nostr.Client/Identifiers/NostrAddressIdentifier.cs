using System.Text;
using Nostr.Client.Messages;
using Nostr.Client.Utils;

namespace Nostr.Client.Identifiers
{
    public class NostrAddressIdentifier : NostrIdentifier
    {
        public NostrAddressIdentifier(string identifier, string? pubkey, string[]? relays, NostrKind? kind)
        {
            Identifier = identifier;
            Pubkey = pubkey;
            Kind = kind;
            Relays = relays ?? Array.Empty<string>();
        }

        public override string Hrp => "naddr";
        public override string Special => Identifier;
        public string Identifier { get; init; }
        public string? Pubkey { get; init; }
        public string[] Relays { get; init; }
        public NostrKind? Kind { get; init; }

        public override string ToBech32()
        {
            var tlvData = new List<(byte, byte[])>
            {
                (SpecialKey, HashExtensions.FromString(Identifier)),
            };
            tlvData.AddRange(Relays.Select(relay => (RelayKey, Encoding.ASCII.GetBytes(relay))));
            if (!string.IsNullOrWhiteSpace(Pubkey))
                tlvData.Add((AuthorKey, Convert.FromHexString(Pubkey)));
            if (Kind != null)
                tlvData.Add((KindKey, WriteKind(Kind.Value)));
            var tlv = NostrIdentifierParser.BuildTlv(tlvData.ToArray());
            return NostrConverter.ToBech32(tlv, Hrp) ?? string.Empty;
        }

        public static NostrAddressIdentifier Parse(byte[] data)
        {
            var tlv = NostrIdentifierParser.ParseTlv(data);
            return new NostrAddressIdentifier(
                FindSpecialString(tlv),
                FindAuthor(tlv),
                FindRelays(tlv),
                FindKind(tlv)
            );
        }
    }
}
