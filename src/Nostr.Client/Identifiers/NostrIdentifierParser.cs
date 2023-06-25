using Nostr.Client.Utils;

namespace Nostr.Client.Identifiers
{
    public static class NostrIdentifierParser
    {
        public static NostrIdentifier? Parse(string bech32)
        {
            var data = NostrConverter.ToHexBytes(bech32, out var hrp);
            if (data == null || !data.Any() || string.IsNullOrWhiteSpace(hrp))
                return null;

            return hrp switch
            {
                "nprofile" => NostrProfileIdentifier.Parse(data),
                "nevent" => NostrEventIdentifier.Parse(data),
                "nrelay" => NostrRelayIdentifier.Parse(data),
                "naddr" => NostrAddressIdentifier.Parse(data),
                _ => throw new InvalidOperationException(
                    $"Bech32 {hrp} identifier not yet supported, contact library maintainers")
            };
        }

        internal static IReadOnlyCollection<KeyValuePair<byte, byte[]>> ParseTlv(byte[] tlvData)
        {
            var result = new List<KeyValuePair<byte, byte[]>>();
            var pos = 0;
            while (pos < tlvData.Length)
            {
                var tag = tlvData[pos++];
                int length = tlvData[pos++];

                // handle extended length encoding
                if ((length & 0x80) != 0)
                {
                    int lengthBytes = length & 0x7F;
                    length = 0;
                    for (int i = 0; i < lengthBytes; i++)
                    {
                        length = (length << 8) + tlvData[pos++];
                    }
                }

                var value = new byte[length];
                Array.Copy(tlvData, pos, value, 0, length);
                result.Add(new KeyValuePair<byte, byte[]>(tag, value));
                pos += length;
            }

            return result;
        }

        internal static byte[] BuildTlv((byte, byte[])[] tlvList)
        {
            var result = new List<byte>();

            foreach (var item in tlvList)
            {
                var tag = item.Item1;
                var value = item.Item2;
                var length = value.Length;

                // handle extended length encoding
                var lengthBytes = length > 127 ? (byte)Math.Ceiling(length / 256.0) : (byte)0;
                if (lengthBytes > 0)
                {
                    length = (int)Math.Pow(256, lengthBytes) + length;
                }

                result.Add(tag);
                if (lengthBytes > 0)
                {
                    result.Add((byte)(0x80 | lengthBytes));
                    for (int i = lengthBytes - 1; i >= 0; i--)
                    {
                        result.Add((byte)(length / (int)Math.Pow(256, i)));
                    }
                }
                else
                {
                    result.Add((byte)length);
                }

                result.AddRange(value);
            }

            return result.ToArray();
        }
    }
}
