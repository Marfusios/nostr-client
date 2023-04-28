using System.Reactive.Linq;
using Newtonsoft.Json;
using Nostr.Client.Client;
using Nostr.Client.Messages;
using Nostr.Client.Messages.Contacts;
using Nostr.Client.Messages.Direct;
using Nostr.Client.Messages.Metadata;
using Nostr.Client.Requests;
using Serilog;

namespace Nostr.Client.Sample.Console
{
    internal class NostrViewer
    {
        private readonly INostrClient _client;

        public NostrViewer(INostrClient client)
        {
            _client = client;
        }

        public void Subscribe()
        {
            var events = _client.Streams.EventStream
                .Where(x => x.Event != null);

            events.Subscribe(x =>
                Log.Information("[{relay}] {kind}: {content}", x.CommunicatorName, x.Event?.Kind, x.Event?.Content));

            events
                .Select(x => x.Event!)
                .OfType<NostrMetadataEvent>()
                .Subscribe(x =>
                    Log.Information("Name: {name}, about: {about}", x.Metadata?.Name, x.Metadata?.About));

            events
                .Select(x => x.Event!)
                .OfType<NostrContactEvent>()
                .Subscribe(x =>
                {
                    foreach (var relay in x.Relays)
                    {
                        Log.Information("Relay: {url}, write: {about}, read: {read}", relay.Key, relay.Value.Write,
                            relay.Value.Read);
                    }
                });

            events
                .Select(x => x.Event!)
                .OfType<NostrEncryptedEvent>()
                .Subscribe(x =>
                {
                    Log.Information("DM message: {content}, from {from} to {to}", x.EncryptedContent, x.Pubkey?[..4],
                            x.RecipientPubkey?[..4]);
                });

            _client.Streams.NoticeStream.Subscribe(x => Log.Information("[{relay}] Notice: {message}", x.CommunicatorName, x.Message));
            _client.Streams.EoseStream.Subscribe(x => Log.Information("[{relay}] EOSE of subscription {subscription}", x.CommunicatorName, x.Subscription));
            _client.Streams.OkStream.Subscribe(x => Log.Information("[{relay}] OK {subscription} success: {success} {message}", x.CommunicatorName, x.EventId, x.Accepted, x.Message));
            _client.Streams.UnknownMessageStream.Subscribe(x => Log.Information("[{relay}] Unknown {messageType} message, data: {data}", x.CommunicatorName, x.MessageType, JsonConvert.SerializeObject(x.AdditionalData)));
            _client.Streams.UnknownRawStream.Subscribe(x => Log.Warning("[{relay}] Unknown data: {data}", x.CommunicatorName, x.Message?.ToString()));
        }

        public void SendRequests()
        {
            _client.Send(new NostrRequest("timeline:pubkey:follows", new NostrFilter
            {
                Authors = new[]
                {
                    "6b75a3b4832f265989254ca560b700da3343d707d2319e7a45f4e01afe4a0c31",
                    "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
                    "63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed",
                    "e9e4276490374a0daf7759fd5f475deff6ffb9b0fc5fa98c902b5f4b2fe3bba2",
                    "604e96e099936a104883958b040b47672e0f048c98ac793f37ffe4c720279eb2",
                    "7b6461d02c6f0be1cacdcf968c4246105a2db51c7770993bf8bb25e59cedffa7",
                    "559dc217d58a74982396fea8b4e9af4b6fc9c96f11abb134da285fec028658fd",
                    "23518fb6a27dc83e475eca500600e2160c71e554786dfda9658d5d9f57819b66",
                    "91c9a5e1a9744114c6fe2d61ae4de82629eaaa0fb52f48288093c7e7e036f832",
                    "c4eabae1be3cf657bc1855ee05e69de9f059cb7a059227168b80b89761cbc4e0",
                    "04c915daefee38317fa734444acee390a8269fe5810b2241e5e6dd343dfbecc9",
                    "339d7804b6a69b7ef05a169d72ca3e977f64eb00ab6eedf21af0a2c2327691b3",
                    "85080d3bad70ccdcd7f74c29a44f55bb85cbcd3dd0cbb957da1d215bdb931204",
                    "020f2d21ae09bf35fcdfb65decf1478b846f5f728ab30c5eaabcd6d081a81c3e",
                    "6e468422dfb74a5738702a8823b9b28168abab8655faacb6853cd0ee15deee93",
                    "3bf0c63fcb93463407af97a5e5ee64fa883d107ef9e558472c4eb9aaaefa459d",
                    "e33fe65f1fde44c6dc17eeb38fdad0fceaf1cae8722084332ed1e32496291d42",
                    "a341f45ff9758f570a21b000c17d4e53a3a497c8397f26c0e6d61e5acffc7a98",
                    "83e818dfbeccea56b0f551576b3fd39a7a50e1d8159343500368fa085ccd964b",
                    "7fa56f5d6962ab1e3cd424e758c3002b8665f7b0d8dcee9fe9e288d7751ac194",
                    "46bb7d86f84da649ff8a2404533de360d7baa6fe48fc03f779848c5f4c95d3b9",
                    "a575563c6b5b7a029f472e859ec2af026938cd8a03cf0fe2e6b82472b54aa638",
                    "7e8575871843980ffee6f8bcd37cc381589b5653bb8a1b3e585bf5e2a5c15f78",
                    "d27790fcb3f9afa0d709b2e9c5995151bc5ad008079bd0a474aa101d80e0eed3"
                },
                Kinds = new[]
                {
                    NostrKind.Metadata,
                    NostrKind.ShortTextNote,
                    NostrKind.EncryptedDm,
                    NostrKind.Reaction,
                    NostrKind.Contacts,
                    NostrKind.RecommendRelay,
                    NostrKind.EventDeletion,
                    NostrKind.Reporting,
                    NostrKind.ClientAuthentication
                },
                Since = DateTime.UtcNow.AddHours(-12),
                Until = DateTime.UtcNow.AddHours(4)
            }));
        }
    }
}
