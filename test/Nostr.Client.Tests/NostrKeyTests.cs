using Nostr.Client.Keys;
using Nostr.Client.Utils;

namespace Nostr.Client.Tests
{
    public class NostrKeyTests
    {
        [Theory]
        [InlineData("0cce8c841774e499b15babdd50b4c3f9c0fc828eefe1508fc4910ed5f6bea241", "89fa4b8bce7d7ba022dec50e8f7cfae055514010dc6226bb30dc8f7d17ea73fd")]
        [InlineData("34a3e00bcd58050ae4ee8227389240266948dfb7d88239f1e89adc15427dfa05", "7d89d8818771bd6b3e10b868f52f9ddc0014fa0207d51729cf3acd2c7944663b")]
        public void Construct_FromPrivateKeyHex_ShouldBeCorrect(string privateKey, string expectedPublicKey)
        {
            var pair = NostrKeyPair.From(NostrPrivateKey.FromHex(privateKey));

            Assert.Equal(expectedPublicKey, pair.PublicKey.Hex);
            Assert.Equal(NostrConverter.ToNpub(pair.PublicKey.Hex), pair.PublicKey.Bech32);
            Assert.Equal(NostrConverter.ToNsec(privateKey), pair.PrivateKey.Bech32);
        }

        [Theory]
        [InlineData("nsec1xj37qz7dtqzs4e8wsgnn3yjqye553hahmzprnu0gntwp2snalgzsnv42g5", "npub10kya3qv8wx7kk0sshp502tuamsqpf7szql23w2w08txjc72yvcasg0gpn5")]
        [InlineData("nsec1k0u6cj3c3eyaey7vphy7nrq2eudfdns8qrdkf0j665xagxhf83rs5gkn58", "npub15zwr0rspve52gnj2lhhw3s74nud9yz6qsgsfds3hxmuv52v5ljxsulkqmy")]
        public void Construct_FromPrivateKeyBech32_ShouldBeCorrect(string privateKey, string expectedPublicKey)
        {
            var pair = NostrKeyPair.From(NostrPrivateKey.FromBech32(privateKey));

            Assert.Equal(expectedPublicKey, pair.PublicKey.Bech32);
            Assert.Equal(NostrConverter.ToHex(pair.PublicKey.Bech32, out _), pair.PublicKey.Hex);
            Assert.Equal(NostrConverter.ToHex(pair.PrivateKey.Bech32, out _), pair.PrivateKey.Hex);
        }

        [Fact]
        public void GenerateNew_ShouldWorkCorrectly()
        {
            var random = NostrKeyPair.GenerateNew();
            var publicKeyEc = random.PrivateKey.Ec.CreateXOnlyPubKey();
            var publicKey = NostrPublicKey.FromEc(publicKeyEc);

            Assert.NotNull(random.PrivateKey);
            Assert.NotNull(random.PublicKey);
            Assert.Equal(publicKey, random.PublicKey);
        }

    }
}
