namespace Nostr.Client.Keys
{
    /// <summary>
    /// Holds private and public key pair
    /// </summary>
    public class NostrKeyPair : IEquatable<NostrKeyPair>
    {
        public NostrKeyPair(NostrPrivateKey privateKey, NostrPublicKey publicKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public NostrKeyPair(NostrPrivateKey privateKey)
        {
            PrivateKey = privateKey;
            PublicKey = NostrPublicKey.FromPrivateEc(PrivateKey.Ec);
        }

        public NostrPrivateKey PrivateKey { get; }

        public NostrPublicKey PublicKey { get; }

        public bool Equals(NostrKeyPair? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PrivateKey.Equals(other.PrivateKey) && PublicKey.Equals(other.PublicKey);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NostrKeyPair)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PrivateKey, PublicKey);
        }

        /// <summary>
        /// Create key pair based on private key
        /// </summary>
        public static NostrKeyPair From(NostrPrivateKey privateKey)
        {
            return new NostrKeyPair(privateKey);
        }

        /// <summary>
        /// Generate a new random key pair
        /// </summary>
        public static NostrKeyPair GenerateNew()
        {
            var privateKey = NostrPrivateKey.GenerateNew();
            return new NostrKeyPair(privateKey);
        }
    }
}
