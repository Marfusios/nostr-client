using Nostr.Client.Messages;
using Nostr.Client.Responses;

namespace NostrDebug.Web.Events
{
    public class EventStorage
    {
        private readonly Dictionary<string, NostrEventResponse> _responses = new();
        private readonly Dictionary<string, NostrEvent> _events = new();
        private readonly Dictionary<string, HashSet<string>> _eventToCommunicators = new();

        public void Store(NostrEventResponse? response)
        {
            var eventId = response?.Event?.Id;
            if (eventId == null)
                return;
            _responses[eventId] = response!;

            if (!_eventToCommunicators.ContainsKey(eventId))
                _eventToCommunicators[eventId] = new HashSet<string>();
            _eventToCommunicators[eventId].Add(response!.CommunicatorName);
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
            return _events.TryGetValue(eventId, out var @event) ?
                @event :
                null;
        }

        public string[] FindCommunicators(string? eventId)
        {
            var key = eventId ?? string.Empty;
            return _eventToCommunicators.TryGetValue(key, out var communicators) ?
                communicators.ToArray() :
                Array.Empty<string>();
        }

        public void Clear()
        {
            _events.Clear();
            _responses.Clear();
        }
    }
}
