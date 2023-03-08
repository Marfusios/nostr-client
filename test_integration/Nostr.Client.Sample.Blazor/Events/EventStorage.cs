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
            var clone = ev!.DeepClone();
            _events[eventId] = clone;
        }

        public NostrEventResponse? FindResponse(string eventId)
        {
            return _events.ContainsKey(eventId) ?
                _responses[eventId] :
                null;
        }

        public NostrEvent? FindEvent(string eventId)
        {
            return _events.ContainsKey(eventId) ?
                _events[eventId] :
                null;
        }

        public void Clear()
        {
            _events.Clear();
            _responses.Clear();
        }
    }
}
