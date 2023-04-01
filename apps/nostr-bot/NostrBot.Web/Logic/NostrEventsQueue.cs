using System.Threading.Channels;
using Nostr.Client.Responses;

namespace NostrBot.Web.Logic
{
    public class NostrEventsQueue
    {
        private readonly Channel<NostrEventResponse> _channel;
        public NostrEventsQueue(Channel<NostrEventResponse> channel)
        {
            _channel = channel;
        }

        public ChannelReader<NostrEventResponse> Reader => _channel.Reader;

        public ChannelWriter<NostrEventResponse> Writer => _channel.Writer;
    }
}
