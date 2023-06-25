using Nostr.Client.Utils;

namespace Nostr.Client.Identifiers
{
    public class NostrRelayIdentifier : NostrIdentifier
    {
        public NostrRelayIdentifier(string relay)
        {
            Relay = relay;
        }

        public override string Hrp => "nrelay";
        public override string Special => Relay;
        public string Relay { get; init; }

        public override string ToBech32()
        {
            var tlvData = new List<(byte, byte[])>
            {
                (SpecialKey, HashExtensions.FromString(Relay)),
            };
            var tlv = NostrIdentifierParser.BuildTlv(tlvData.ToArray());
            return NostrConverter.ToBech32(tlv, Hrp) ?? string.Empty;
        }

        public static NostrRelayIdentifier Parse(byte[] data)
        {
            var tlv = NostrIdentifierParser.ParseTlv(data);
            return new NostrRelayIdentifier(
                FindSpecialString(tlv)
            );
        }
    }
}
