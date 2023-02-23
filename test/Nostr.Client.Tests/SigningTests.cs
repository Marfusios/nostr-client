using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Responses;

namespace Nostr.Client.Tests
{
    public class SigningTests
    {
        [Fact]
        public void ComputeId_SimpleEvent_ShouldWork()
        {
            var ev = new NostrEvent
            {
                Id = "dc24dd0c6ac6443c280f912caf091752bb3ba2c59cfc308b4238817ff7f82ff8",
                Pubkey = "604e96e099936a104883958b040b47672e0f048c98ac793f37ffe4c720279eb2",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 16, 48, 20, DateTimeKind.Utc),
                Content = "gm\nhttps://youtu.be/ZO-jQjEdK9w",
                Tags = null,
                Sig = "80255d683b5010639cce2c9c35231636933b27d51cabe17eb157703d8d1c489408d588b60b2a9a36f711d6fdef281e5110de3827ce3e1c571cb4eb89c69213a4"
            };

            var computed = ev.ComputeId();
            Assert.Equal(ev.Id, computed);
        }

        [Fact]
        public void ComputeId_EventWithTags_ShouldWork()
        {
            var ev = new NostrEvent
            {
                Id = "78f3298e6af56007aea5e32a2369300a7ef4400f6edc55d57384c809a18013d9",
                Pubkey = "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 18, 14, 41, DateTimeKind.Utc),
                Content = "surprize",
                Tags = new[]
                {
                    new NostrEventTag("e", "4eab0a10fa8aa611d55b7f8ea41f7756c69284362a8c8cccf2ed2dc0362d7aad"),
                    new NostrEventTag("p", "7f3b464b9ff3623630485060cbda3a7790131c5339a7803bde8feb79a5e1b06a")
                },
                Sig = "b0d0b6ec2162bf1beb2ef80fba65dc196c27a0b416eddd4a61aee08777a8364cc9b630a50c22a06fced9aea94cc64fd8486ec66aa3141c21c5a6cdb7c41786bf"
            };

            var computed = ev.ComputeId();
            Assert.Equal(ev.Id, computed);
        }

        [Fact]
        public void Sign_SimpleEvent_ShouldWork()
        {
            var ev = new NostrEvent
            {
                Id = "a54b4cf414797c2e8f3091493d367bba7d3f8e85482770dd44000a6bd2e9c9dd",
                Pubkey = "a7319aeee29127d6bd1fb0562cf616e365a2b10d635a1cb9a86a23df4add73d7",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 21, 13, 27, DateTimeKind.Utc),
                Content = "Test 1",
                Tags = null,
                Sig = "be891dacdc4259b777e3e23ea601ae2a9c51139599521a98ee4ca0f36aa4cb6eafb616bc35a3bba06597a7840a63794c037461fab460ef392e87850aba0f2e4b"
            };

            Assert.True(ev.IsSignatureValid());

            var privateKey =
                NostrPrivateKey.FromBech32("nsec10jj7d532su7gunn6rnpezgwyr0nvd55r4whppj4t64xux03sxvwsd4n5my");

            var newSignature = ev.ComputeSignature(privateKey);
            var evClone = ev.DeepClone(ev.Id, newSignature);

            Assert.True(evClone.IsSignatureValid());
        }

        [Fact]
        public void Sign_EventWithTags_ShouldWork()
        {
            var ev = new NostrEvent
            {
                Id = "78f3298e6af56007aea5e32a2369300a7ef4400f6edc55d57384c809a18013d9",
                Pubkey = "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 18, 14, 41, DateTimeKind.Utc),
                Content = "surprize",
                Tags = new[]
                {
                    new NostrEventTag("e", "4eab0a10fa8aa611d55b7f8ea41f7756c69284362a8c8cccf2ed2dc0362d7aad"),
                    new NostrEventTag("p", "7f3b464b9ff3623630485060cbda3a7790131c5339a7803bde8feb79a5e1b06a")
                },
                Sig = "b0d0b6ec2162bf1beb2ef80fba65dc196c27a0b416eddd4a61aee08777a8364cc9b630a50c22a06fced9aea94cc64fd8486ec66aa3141c21c5a6cdb7c41786bf"
            };

            Assert.True(ev.IsSignatureValid());

            var privateKey =
                NostrPrivateKey.FromBech32("nsec10jj7d532su7gunn6rnpezgwyr0nvd55r4whppj4t64xux03sxvwsd4n5my");

            var signedEvent = ev.Sign(privateKey);

            Assert.True(signedEvent.IsSignatureValid());
        }

        [Fact]
        public void Sign_InvalidSignature_ShouldFail()
        {
            var ev = new NostrEvent
            {
                Id = "dc24dd0c6ac6443c280f912caf091752bb3ba2c59cfc308b4238817ff7f82ff8",
                Pubkey = "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 16, 48, 20, DateTimeKind.Utc),
                Content = "gm\nhttps://youtu.be/ZO-jQjEdK9w",
                Tags = null,
                Sig = "b0d0b6ec2162bf1beb2ef80fba65dc196c27a0b416eddd4a61aee08777a8364cc9b630a50c22a06fced9aea94cc64fd8486ec66aa3141c21c5a6cdb7c41786bf"
            };

            Assert.False(ev.IsSignatureValid());
        }

        [Fact]
        public void Sign_NewSignatureWithOldPubKey_ShouldFail()
        {
            var ev = new NostrEvent
            {
                Id = "78f3298e6af56007aea5e32a2369300a7ef4400f6edc55d57384c809a18013d9",
                Pubkey = "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 18, 14, 41, DateTimeKind.Utc),
                Content = "surprize",
                Tags = new[]
                {
                    new NostrEventTag("e", "4eab0a10fa8aa611d55b7f8ea41f7756c69284362a8c8cccf2ed2dc0362d7aad"),
                    new NostrEventTag("p", "7f3b464b9ff3623630485060cbda3a7790131c5339a7803bde8feb79a5e1b06a")
                },
                Sig = "b0d0b6ec2162bf1beb2ef80fba65dc196c27a0b416eddd4a61aee08777a8364cc9b630a50c22a06fced9aea94cc64fd8486ec66aa3141c21c5a6cdb7c41786bf"
            };

            Assert.True(ev.IsSignatureValid());

            var privateKey =
                NostrPrivateKey.FromBech32("nsec10jj7d532su7gunn6rnpezgwyr0nvd55r4whppj4t64xux03sxvwsd4n5my");

            var newSignature = ev.ComputeSignature(privateKey);
            var evCloneWithUnchangedPubKey = ev.DeepClone(ev.Id, newSignature);

            Assert.False(evCloneWithUnchangedPubKey.IsSignatureValid());
        }
    }
}
