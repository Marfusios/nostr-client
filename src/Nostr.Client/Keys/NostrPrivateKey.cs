using NBitcoin.Secp256k1;
using Nostr.Client.Utils;
using System.Security.Cryptography;

namespace Nostr.Client.Keys
{
    /// <summary>
    /// Private key structure that holds various formats
    /// </summary>
    public class NostrPrivateKey : IEquatable<NostrPrivateKey>
    {
        private NostrPrivateKey(string hex, string bech32, ECPrivKey ec)
        {
            Hex = hex;
            Bech32 = bech32;
            Ec = ec;
        }

        public string Hex { get; }

        public string Bech32 { get; }

        public ECPrivKey Ec { get; }

        /// <summary>
        /// Derive public key from this private key
        /// </summary>
        public NostrPublicKey DerivePublicKey()
        {
            return NostrPublicKey.FromPrivateEc(Ec);
        }

        /// <summary>
        /// Derive shared key between secret and public one
        /// </summary>
        public NostrPublicKey DeriveSharedKey(NostrPublicKey publicKey)
        {
            // 32 + 1 byte for the compression (0x02) prefix.
            Span<byte> input = stackalloc byte[33];
            input[0] = 0x02;
            publicKey.Ec.WriteToSpan(input[1..]);

            var success = Context.Instance.TryCreatePubKey(input, out var prefixedPublicKey);
            if (!success || prefixedPublicKey == null)
                throw new InvalidOperationException("Can't create prefixed public key");

            var sharedKey = prefixedPublicKey.GetSharedPubkey(Ec);
            if (sharedKey == null)
                throw new InvalidOperationException("Can't create shared public key");

            return NostrPublicKey.FromEc(sharedKey.ToXOnlyPubKey());
        }

        /// <summary>
        /// Sign hex with the private key. 
        /// Returns signature in hex format.
        /// </summary>
        public string? SignHex(string? hex)
        {
            if (hex == null)
                return null;

            var hexBytes = HexExtensions.ToByteArray(hex);
            return !Ec.TrySignBIP340(hexBytes, null, out var signature) ?
                null :
                signature.ToBytes().ToHex();
        }

        public bool Equals(NostrPrivateKey? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Hex, other.Hex, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Bech32, other.Bech32, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NostrPrivateKey)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Hex, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Bech32, StringComparer.OrdinalIgnoreCase);
            return hashCode.ToHashCode();
        }

        public static NostrPrivateKey FromHex(string hex)
        {
            var ec = ECPrivKey.Create(HexExtensions.ToByteArray(hex));
            var bech32 = NostrConverter.ToNsec(hex) ?? string.Empty;
            return new NostrPrivateKey(hex, bech32, ec);
        }

        public static NostrPrivateKey FromBech32(string bech32)
        {
            var hex = NostrConverter.ToHex(bech32, out var hrp);
            if (!"nsec".Equals(hrp, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Provided bech32 key is not 'nsec'", nameof(bech32));
            return FromHex(hex);
        }

        public static NostrPrivateKey FromEc(ECPrivKey ec)
        {
            var hex = ToHex(ec);
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Provided ec key is not correct", nameof(ec));
            return FromHex(hex);
        }

        public static string ToHex(ECPrivKey key)
        {
            Span<byte> keySpan = stackalloc byte[65];
            key.WriteToSpan(keySpan);
            var keyHex = keySpan.ToHex();
            return keyHex[..64];
        }

        /// <summary>
        /// Generate a new random private key
        /// </summary>
        public static NostrPrivateKey GenerateNew()
        {
            using var randomGen = RandomNumberGenerator.Create();
            var randomBytes = new byte[32];
            randomGen.GetBytes(randomBytes);
            var randomHex = randomBytes.ToHex();
            return FromHex(randomHex);
        }
    }
}
