using System.Reactive.Linq;
using System.Reactive.Subjects;
using Blazored.LocalStorage;
using Nostr.Client.Client;
using NostrDebug.Web.Utils;
using Websocket.Client;

namespace NostrDebug.Web.Relay
{
    public class RelayList : IDisposable
    {
        private static readonly HashSet<string> DefaultRelays = new(new[]{
            "wss://nos.lol",
            "wss://relay.damus.io",
            "wss://nostr-pub.wellorder.net",
            "wss://nostr.wine",
            "wss://relay.snort.social"
        });

        private static readonly string RelaysToSelectKey = "relays-to-select";

        private readonly ILogger<RelayConnection> _logger;
        private readonly ISyncLocalStorageService _localStorage;
        private readonly HashSet<RelayConnection> _relays = new();
        private readonly HashSet<string> _relaysToSelect = new();

        private readonly ReplaySubject<string> _historySubject = new(200);
        private readonly Subject<bool> _connectionSubject = new();

        public RelayList(ILogger<RelayConnection> logger, ILogger<NostrWebsocketClient> loggerClient, ISyncLocalStorageService localStorage)
        {
            _logger = logger;
            _localStorage = localStorage;

            InitRelaysToSelect();

            Client = new NostrMultiWebsocketClient(loggerClient);
            AddNextRelay();
        }

        public NostrMultiWebsocketClient Client { get; }

        public IReadOnlyCollection<RelayConnection> Relays => _relays;

        public IReadOnlyCollection<string> RelaysToSelect => _relaysToSelect;

        public bool IsConnecting => _relays.Any(x => x.IsConnecting);

        public bool IsAnyConnected => _relays.Any(x => x.IsConnected);

        public bool AreAllConnected => _relays.Where(x => x.IsStarted).All(x => x.IsConnected);

        public int ReceivedMessagesCount => _relays.Sum(x => x.ReceivedMessagesCount);

        public IObservable<string> HistoryStream => _historySubject.AsObservable();
        public IObservable<bool> ConnectionStream => _connectionSubject.AsObservable();
        public IObservable<ResponseMessage> MessageReceivedStream => _relays.Select(x => x.Communicator.MessageReceived).Merge();

        public void Dispose()
        {
            Client.Dispose();

            foreach (var relay in _relays)
            {
                relay.Dispose();
            }
        }

        public bool Connect(RelayConnection relay)
        {
            if (Client.FindClient(relay.Communicator) == null)
            {
                Client.RegisterCommunicator(relay.Communicator);
            }

            StoreRelaysToSelect();
            AddNextRelay();
            return true;
        }

        private void AddNextRelay()
        {
            var nonUsedExists = _relays.Any(x => !x.IsUsed);
            if (nonUsedExists)
            {
                return;
            }

            var connectedRelays = _relays.Select(x => x.RelayUrl);
            var notConnectedRelays = _relaysToSelect.Select(x => new Uri(x)).Except(connectedRelays).ToArray();
            var firstRelay = notConnectedRelays.FirstOrDefault()?.ToString() ?? _relaysToSelect.First();

            var relay = new RelayConnection(_logger, new Uri(firstRelay));
            relay.HistoryStream.Subscribe(_historySubject);
            relay.ConnectionStream.Subscribe(_connectionSubject);

            _relays.Add(relay);
        }

        private void InitRelaysToSelect()
        {
            _relaysToSelect.AddRange(DefaultRelays);
            var storedRelays = _localStorage.GetItem<string[]?>(RelaysToSelectKey);
            if (storedRelays == null)
                return;
            _relaysToSelect.AddRange(storedRelays);
        }

        private void StoreRelaysToSelect()
        {
            if (!_relays.Any())
                return;
            var relaysToStore = _relays.Select(x => x.RelayUrl.ToString()).ToArray();
            var storedRelays = _localStorage.GetItem<string[]?>(RelaysToSelectKey) ?? Array.Empty<string>();

            var nonDefault = storedRelays
                .Concat(relaysToStore)
                .Select(x => x.TrimEnd('/'))
                .Except(DefaultRelays)
                .ToHashSet();
            _localStorage.SetItem(RelaysToSelectKey, nonDefault.ToArray());
        }
    }
}
