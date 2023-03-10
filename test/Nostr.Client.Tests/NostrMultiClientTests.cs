using Nostr.Client.Client;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Responses;
using Nostr.Client.Tests.Fakes;

namespace Nostr.Client.Tests
{
    public class NostrMultiClientTests
    {
        [Fact]
        public void SendEvent_ShouldBeForwardedToEveryCommunicator()
        {
            var communicator1 = new NostrFakeCommunicator();
            var communicator2 = new NostrFakeCommunicator();
            var communicator3 = new NostrFakeCommunicator();

            var client = new NostrMultiWebsocketClient(null, communicator1, communicator2, communicator3);

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

            Assert.Equal(expected, communicator1.SentMessages.Single());
            Assert.Equal(expected, communicator2.SentMessages.Single());
            Assert.Equal(expected, communicator3.SentMessages.Single());
        }

        [Fact]
        public void Streams_ShouldForwardMessages()
        {
            var receivedEvents = new List<NostrEventResponse?>();
            var communicator1 = new NostrFakeCommunicator();
            var communicator2 = new NostrFakeCommunicator();
            var communicator3 = new NostrFakeCommunicator();

            var client = new NostrMultiWebsocketClient(null, communicator1, communicator2, communicator3);

            var ev = new NostrEvent
            {
                Id = "cff674d3d7e741120883a9aacee054d69830dd0e6e2ff549d5b6b3ceaa88df4d",
                Pubkey = "819d714d18439d8add3a5a89d4196c0f858f5e5913b547764b0eeeacbbb48028",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 23, 09, 07, 56, DateTimeKind.Utc),
                Content = "Test message",
                Sig = "303fd07c2f5345339d6d534ef1efb6a2619f2fafdf301627e4a7a68e7ab5486cd435b568616c341e948f9e09b5b8e0df5fa2ac3a3404b4f7aebc3db88562014a"
            };

            client.Streams.EventStream.Subscribe(receivedEvents.Add);

            client.Send(new NostrEventRequest(ev));

            Assert.Equal(3, receivedEvents.Count);
            Assert.True(receivedEvents.Any(x => x?.CommunicatorName == communicator2.Name));
            Assert.NotNull(client.FindClient(communicator3.Name));
        }

        [Fact]
        public void RemoveRegistration_ShouldStopForwardingMessages()
        {
            var receivedEvents = new List<NostrEventResponse?>();
            var communicator1 = new NostrFakeCommunicator();
            var communicator2 = new NostrFakeCommunicator();
            var communicator3 = new NostrFakeCommunicator();

            var client = new NostrMultiWebsocketClient(null, communicator1, communicator2, communicator3);

            var ev = new NostrEvent
            {
                Id = "cff674d3d7e741120883a9aacee054d69830dd0e6e2ff549d5b6b3ceaa88df4d",
                Pubkey = "819d714d18439d8add3a5a89d4196c0f858f5e5913b547764b0eeeacbbb48028",
                Kind = NostrKind.ShortTextNote,
                CreatedAt = new DateTime(2023, 2, 23, 09, 07, 56, DateTimeKind.Utc),
                Content = "Test message",
                Sig = "303fd07c2f5345339d6d534ef1efb6a2619f2fafdf301627e4a7a68e7ab5486cd435b568616c341e948f9e09b5b8e0df5fa2ac3a3404b4f7aebc3db88562014a"
            };

            client.Streams.EventStream.Subscribe(receivedEvents.Add);

            client.Send(new NostrEventRequest(ev));

            Assert.Equal(3, receivedEvents.Count);
            Assert.True(receivedEvents.Any(x => x?.CommunicatorName == communicator2.Name));
            Assert.NotNull(client.FindClient(communicator2.Name));

            client.RemoveRegistration(communicator2.Name);
            receivedEvents.Clear();

            client.Send(new NostrEventRequest(ev));

            Assert.Equal(2, receivedEvents.Count);
            Assert.False(receivedEvents.Any(x => x?.CommunicatorName == communicator2.Name));
            Assert.Null(client.FindClient(communicator2.Name));
        }
    }
}
