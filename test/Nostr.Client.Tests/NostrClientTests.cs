using Nostr.Client.Client;
using Nostr.Client.Json;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Responses;
using Nostr.Client.Tests.Fakes;
using Websocket.Client;

namespace Nostr.Client.Tests
{
    public class NostrClientTests
    {
        [Fact]
        public void SimpleEvent_ShouldBeCorrectlySerialized()
        {
            var ev = new NostrEvent
            {
                Id = "cff674d3d7e741120883a9aacee054d69830dd0e6e2ff549d5b6b3ceaa88df4d",
                Pubkey = "819d714d18439d8add3a5a89d4196c0f858f5e5913b547764b0eeeacbbb48028",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 23, 09, 07, 56, DateTimeKind.Utc),
                Content = "Test message. \n\nMulti line. !@#$%^&*()\n\nhttps://nostr.mkotas.cz \n\n“✓” \"\" '' OK",
                Sig = "303fd07c2f5345339d6d534ef1efb6a2619f2fafdf301627e4a7a68e7ab5486cd435b568616c341e948f9e09b5b8e0df5fa2ac3a3404b4f7aebc3db88562014a"
            };

            var serialized = NostrJson.Serialize(ev);
            var expected = "{" +
                           "\"id\":\"cff674d3d7e741120883a9aacee054d69830dd0e6e2ff549d5b6b3ceaa88df4d\"," +
                           "\"pubkey\":\"819d714d18439d8add3a5a89d4196c0f858f5e5913b547764b0eeeacbbb48028\"," +
                           "\"created_at\":1677143276," +
                           "\"kind\":1," +
                           "\"tags\":[]," +
                           "\"content\":\"Test message. \\n\\nMulti line. !@#$%^&*()\\n\\nhttps://nostr.mkotas.cz \\n\\n“✓” \\\"\\\" '' OK\"," +
                           "\"sig\":\"303fd07c2f5345339d6d534ef1efb6a2619f2fafdf301627e4a7a68e7ab5486cd435b568616c341e948f9e09b5b8e0df5fa2ac3a3404b4f7aebc3db88562014a\"" +
                           "}";

            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SendSimpleEvent_ShouldBeCorrectlySerialized()
        {
            var communicator = new NostrFakeCommunicator();
            var client = new NostrWebsocketClient(communicator, null);

            var ev = new NostrEvent
            {
                Id = "cff674d3d7e741120883a9aacee054d69830dd0e6e2ff549d5b6b3ceaa88df4d",
                Pubkey = "819d714d18439d8add3a5a89d4196c0f858f5e5913b547764b0eeeacbbb48028",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 23, 09, 07, 56, DateTimeKind.Utc),
                Content = "Test message. \n\nMulti line. !@#$%^&*()\n\nhttps://nostr.mkotas.cz \n\n“✓” \"\" '' OK",
                Sig = "303fd07c2f5345339d6d534ef1efb6a2619f2fafdf301627e4a7a68e7ab5486cd435b568616c341e948f9e09b5b8e0df5fa2ac3a3404b4f7aebc3db88562014a"
            };

            client.Send(new NostrEventRequest(ev));

            var sent = communicator.SentMessages.Single();
            var expected = "[\"EVENT\"," +
                           "{" +
                           "\"id\":\"cff674d3d7e741120883a9aacee054d69830dd0e6e2ff549d5b6b3ceaa88df4d\"," +
                           "\"pubkey\":\"819d714d18439d8add3a5a89d4196c0f858f5e5913b547764b0eeeacbbb48028\"," +
                           "\"created_at\":1677143276," +
                           "\"kind\":1," +
                           "\"tags\":[]," +
                           "\"content\":\"Test message. \\n\\nMulti line. !@#$%^&*()\\n\\nhttps://nostr.mkotas.cz \\n\\n“✓” \\\"\\\" '' OK\"," +
                           "\"sig\":\"303fd07c2f5345339d6d534ef1efb6a2619f2fafdf301627e4a7a68e7ab5486cd435b568616c341e948f9e09b5b8e0df5fa2ac3a3404b4f7aebc3db88562014a\"" +
                           "}]";

            Assert.Equal(expected, sent);
        }

        [Fact]
        public void SendComplexEvent_ShouldBeCorrectlySerialized()
        {
            var communicator = new NostrFakeCommunicator();
            var client = new NostrWebsocketClient(communicator, null);

            var ev = new NostrEvent
            {
                Id = "78f3298e6af56007aea5e32a2369300a7ef4400f6edc55d57384c809a18013d9",
                Pubkey = "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 22, 18, 14, 41, DateTimeKind.Utc),
                Content = "surprize",
                Tags = new(
                    new NostrEventTag("e", "4eab0a10fa8aa611d55b7f8ea41f7756c69284362a8c8cccf2ed2dc0362d7aad"),
                    new NostrEventTag("p", "7f3b464b9ff3623630485060cbda3a7790131c5339a7803bde8feb79a5e1b06a")
                ),
                Sig = "b0d0b6ec2162bf1beb2ef80fba65dc196c27a0b416eddd4a61aee08777a8364cc9b630a50c22a06fced9aea94cc64fd8486ec66aa3141c21c5a6cdb7c41786bf"
            };

            client.Send(new NostrEventRequest(ev));

            var sent = communicator.SentMessages.Single();
            var expected = "[\"EVENT\"," +
                           "{" +
                           "\"id\":\"78f3298e6af56007aea5e32a2369300a7ef4400f6edc55d57384c809a18013d9\"," +
                           "\"pubkey\":\"82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2\"," +
                           "\"created_at\":1677089681," +
                           "\"kind\":1," +
                           "\"tags\":[" +
                           "[\"e\",\"4eab0a10fa8aa611d55b7f8ea41f7756c69284362a8c8cccf2ed2dc0362d7aad\"]," +
                           "[\"p\",\"7f3b464b9ff3623630485060cbda3a7790131c5339a7803bde8feb79a5e1b06a\"]" +
                           "]," +
                           "\"content\":\"surprize\"," +
                           "\"sig\":\"b0d0b6ec2162bf1beb2ef80fba65dc196c27a0b416eddd4a61aee08777a8364cc9b630a50c22a06fced9aea94cc64fd8486ec66aa3141c21c5a6cdb7c41786bf\"" +
                           "}]";

            Assert.Equal(expected, sent);
        }

        [Fact]
        public void ComplexEvent_ShouldBeCorrectlyDeserialized()
        {
            var eventSerialized =
                "{\"pubkey\":\"63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed\",\"kind\":27235,\"created_at\":1680610669,\"content\":\"\",\"tags\":[[\"url\",\"http://localhost:5097/api/v1/subscription\"],[\"method\",\"GET\"]],\"id\":\"8897fc803723b4c6d3f9740a01369971b3b59dbc8d2eb0d53853480663c55ae3\",\"sig\":\"741635cb39e2dbdf21defa31d195aeb749708daf0ffbc94bf37aff0f79a2d36f996e9dab67225655eb506146fd10f87792f926e8b84b2238913ab8b114475501\"}";
            var eventDeserialized = NostrJson.Deserialize<NostrEvent>(eventSerialized);

            Assert.NotNull(eventDeserialized);
            Assert.Equal("63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed", eventDeserialized.Pubkey);
        }

        [Fact]
        public void ComplexEventResponse_ShouldBeCorrectlyDeserialized()
        {
            var eventSerialized = "[\"EVENT\",\"subscription:x\"," +
                                  "{" +
                                  "\"pubkey\":\"63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed\"," +
                                  "\"kind\":27235," +
                                  "\"created_at\":1680610669," +
                                  "\"content\":\"\"," +
                                  "\"tags\":[" +
                                  "[\"url\",\"http://localhost:5097/api/v1/subscription\"]," +
                                  "[\"method\",\"GET\"]" +
                                  "]," +
                                  "\"id\":\"8897fc803723b4c6d3f9740a01369971b3b59dbc8d2eb0d53853480663c55ae3\"," +
                                  "\"sig\":\"741635cb39e2dbdf21defa31d195aeb749708daf0ffbc94bf37aff0f79a2d36f996e9dab67225655eb506146fd10f87792f926e8b84b2238913ab8b114475501\"" +
                                  "}]";
            var eventDeserialized = NostrJson.Deserialize<NostrEventResponse>(eventSerialized);
            Assert.NotNull(eventDeserialized);
            Assert.Equal("63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed", eventDeserialized.Event?.Pubkey);
        }


        [Fact]
        public void ReceiveComplexEvent_ShouldBeCorrectlyDeserialized()
        {
            var communicator = new NostrFakeCommunicator();
            var client = new NostrWebsocketClient(communicator, null);

            var received = new List<NostrEventResponse?>();
            client.Streams.EventStream.Subscribe(received.Add);

            var eventSerialized = "[\"EVENT\",\"subscription:x\"," +
                                  "{" +
                                  "\"pubkey\":\"63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed\"," +
                                  "\"kind\":27235," +
                                  "\"created_at\":1680610669," +
                                  "\"content\":\"\"," +
                                  "\"tags\":[" +
                                  "[\"url\",\"http://localhost:5097/api/v1/subscription\"]," +
                                  "[\"method\",\"GET\"]" +
                                  "]," +
                                  "\"id\":\"8897fc803723b4c6d3f9740a01369971b3b59dbc8d2eb0d53853480663c55ae3\"," +
                                  "\"sig\":\"741635cb39e2dbdf21defa31d195aeb749708daf0ffbc94bf37aff0f79a2d36f996e9dab67225655eb506146fd10f87792f926e8b84b2238913ab8b114475501\"" +
                                  "}]";
            communicator.StreamFakeMessage(ResponseMessage.TextMessage(eventSerialized));

            Assert.Single(received);
            Assert.Equal("63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed", received[0]?.Event?.Pubkey);
        }
    }
}
