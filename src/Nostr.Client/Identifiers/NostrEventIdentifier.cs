using System.Text;
using Nostr.Client.Messages;
using Nostr.Client.Utils;

namespace Nostr.Client.Identifiers
{
    public class NostrEventIdentifier : NostrIdentifier
    {
        public NostrEventIdentifier(string eventId, string? pubkey, string[]? relays, NostrKind? kind)
        {
            EventId = eventId;
            Pubkey = pubkey;
            Kind = kind;
            Relays = relays ?? Array.Empty<string>();
        }

        public override string Hrp => "nevent";
        public override string Special => EventId;
        public string EventId { get; init; }
        public string? Pubkey { get; init; }
        public string[] Relays { get; init; }
        public NostrKind? Kind { get; init; }

        public override string ToBech32()
        {
            var tlvData = new List<(byte, byte[])>
            {
                (SpecialKey, Convert.FromHexString(EventId))
            };
            if (!string.IsNullOrWhiteSpace(Pubkey))
                tlvData.Add((AuthorKey, Convert.FromHexString(Pubkey)));
            tlvData.AddRange(Relays.Select(relay => (RelayKey, Encoding.ASCII.GetBytes(relay))));
            if (Kind != null)
                tlvData.Add((KindKey, WriteKind(Kind.Value)));
            var tlv = NostrIdentifierParser.BuildTlv(tlvData.ToArray());
            return NostrConverter.ToBech32(tlv, Hrp) ?? string.Empty;
        }

        public static NostrEventIdentifier Parse(byte[] data)
        {
            var tlv = NostrIdentifierParser.ParseTlv(data);
            return new NostrEventIdentifier(
                FindSpecialHex(tlv),
                FindAuthor(tlv),
                FindRelays(tlv),
                FindKind(tlv)
            );
        }
    }
}
