using Nostr.Client.Messages;
using Nostr.Client.Responses;

namespace Nostr.Client.Sample.Blazor.Events
{
    public class EventStorage
    {
        private readonly Dictionary<string, NostrEventResponse> _responses = new();
        private readonly Dictionary<string, NostrEvent> _events = new();

        public void Store(NostrEventResponse? response)
        {
            var eventId = response?.Event?.Id;
            if (eventId == null)
                return;
            _responses[eventId] = response!;
        }

        public void Store(NostrEvent? ev)
        {
            var eventId = ev?.Id;
            if (eventId == null)
                return;
            _events[eventId] = ev!;
        }

        public NostrEventResponse? FindResponse(string eventId)
        {
            if (_events.ContainsKey(eventId))
                return _responses[eventId];
            return null;
        }

        public NostrEvent? FindEvent(string eventId)
        {
            if (_events.ContainsKey(eventId))
                return _events[eventId];
            return null;
        }

        public void Clear()
        {
            _events.Clear();
            _responses.Clear();
        }
    }
}
