using Nostr.Client.Identifiers;
using Nostr.Client.Messages;

namespace Nostr.Client.Tests
{
    public class NostrIdentifierTests
    {
        [Fact]
        public void NProfile_ShouldBeParsedCorrectly()
        {
            var bech32 =
                "nprofile1qqsrhuxx8l9ex335q7he0f09aej04zpazpl0ne2cgukyawd24mayt8gpp4mhxue69uhhytnc9e3k7mgpz4mhxue69uhkg6nzv9ejuumpv34kytnrdaksjlyr9p";
            var parsed = NostrIdentifierParser.Parse(bech32);
            var parsedProfile = parsed as NostrProfileIdentifier;

            Assert.NotNull(parsed);
            Assert.NotNull(parsedProfile);

            Assert.Equal("3bf0c63fcb93463407af97a5e5ee64fa883d107ef9e558472c4eb9aaaefa459d", parsedProfile.Pubkey);
            Assert.Equal(2, parsedProfile.Relays.Length);
            Assert.Equal("wss://r.x.com", parsedProfile.Relays[0]);
            Assert.Equal("wss://djbas.sadkb.com", parsedProfile.Relays[1]);

            var serialized = parsedProfile.ToBech32();
            Assert.Equal(bech32, serialized);
        }

        [Fact]
        public void NEvent_ShouldBeParsedCorrectly()
        {
            var bech32 =
                "nevent1qqswtpsw630h908nmlqzpef4e2sca3u6mz9tyxgt03pm2wax4d9ck9gpremhxue69uhkummnw3ez6ur4vgh8wetvd3hhyer9wghxuet59uq32amnwvaz7tmjv4kxz7fwv3sk6atn9e5k7tcrwpl2p";
            var parsed = NostrIdentifierParser.Parse(bech32);
            var parsedEvent = parsed as NostrEventIdentifier;

            Assert.NotNull(parsed);
            Assert.NotNull(parsedEvent);

            Assert.Equal("e5860ed45f72bcf3dfc020e535caa18ec79ad88ab2190b7c43b53ba6ab4b8b15", parsedEvent.EventId);
            Assert.Equal(2, parsedEvent.Relays.Length);
            Assert.Equal("wss://nostr-pub.wellorder.net/", parsedEvent.Relays[0]);
            Assert.Equal("wss://relay.damus.io/", parsedEvent.Relays[1]);
            Assert.Null(parsedEvent.Pubkey);

            var serialized = parsedEvent.ToBech32();
            Assert.Equal(bech32, serialized);
        }

        [Fact]
        public void NEvent_Different_ShouldBeParsedCorrectly()
        {
            var bech32 =
                "nevent1qqsgu3du8y8rsxcyehx7vzgjseqk2jvwd96glkne36ulrlfuc7ezy5czypumuen7l8wthtz45p3ftn58pvrs9xlumvkuu2xet8egzkcklqtesqgwwaehxw309ahx7uewd3hkctcl9s8sg";
            var parsed = NostrIdentifierParser.Parse(bech32);
            var parsedEvent = parsed as NostrEventIdentifier;

            Assert.NotNull(parsed);
            Assert.NotNull(parsedEvent);

            Assert.Equal("8e45bc390e381b04cdcde60912864165498e69748fda798eb9f1fd3cc7b22253", parsedEvent.EventId);
            Assert.Equal("wss://nos.lol/", parsedEvent.Relays[0]);
            Assert.Equal("79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798", parsedEvent.Pubkey);

            var serialized = parsedEvent.ToBech32();
            Assert.Equal(bech32, serialized);
        }

        [Fact]
        public void NRelay_ShouldBeParsedCorrectly()
        {
            var bech32 =
                "nrelay1qq08wumn8ghj7mn0wd68yttsw43zuam9d3kx7unyv4ezumn9wshsevr7js";
            var parsed = NostrIdentifierParser.Parse(bech32);
            var parsedEvent = parsed as NostrRelayIdentifier;

            Assert.NotNull(parsed);
            Assert.NotNull(parsedEvent);

            Assert.Equal("wss://nostr-pub.wellorder.net/", parsedEvent.Relay);

            var serialized = parsedEvent.ToBech32();
            Assert.Equal(bech32, serialized);
        }

        [Fact]
        public void NAddress_ShouldBeParsedCorrectly()
        {
            var bech32 =
                "naddr1qqzkjurnw4ksz9thwden5te0wfjkccte9ehx7um5wghx7un8qgs2d90kkcq3nk2jry62dyf50k0h36rhpdtd594my40w9pkal876jxgrqsqqqa28pccpzu";
            var parsed = NostrIdentifierParser.Parse(bech32);
            var parsedEvent = parsed as NostrAddressIdentifier;

            Assert.NotNull(parsed);
            Assert.NotNull(parsedEvent);

            Assert.Equal("ipsum", parsedEvent.Identifier);
            Assert.Equal("wss://relay.nostr.org", parsedEvent.Relays[0]);
            Assert.Equal("a695f6b60119d9521934a691347d9f78e8770b56da16bb255ee286ddf9fda919", parsedEvent.Pubkey);
            Assert.Equal(NostrKind.LongFormContent, parsedEvent.Kind);

            var serialized = parsedEvent.ToBech32();
            Assert.Equal(bech32, serialized);
        }
    }
}
