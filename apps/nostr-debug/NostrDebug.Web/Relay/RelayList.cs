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
            "wss://relay.snort.social",
            "wss://soloco.nl"
        });

        private static readonly string RelaysToSelectKey = "relays-to-select";
        private static readonly string RelaysConnectedKey = "relays-connected";

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
            
            InitConnectedRelays();
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

        private bool ConnectPure(RelayConnection relay)
        {
            if (Client.FindClient(relay.Communicator) == null)
            {
                Client.RegisterCommunicator(relay.Communicator);
            }
            
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
            var firstRelayUrl = notConnectedRelays.FirstOrDefault()?.ToString() ?? _relaysToSelect.First();

            CreateRelay(firstRelayUrl);
        }

        private RelayConnection CreateRelay(string relayUrl)
        {
            var relay = new RelayConnection(_logger, new Uri(relayUrl));
            relay.HistoryStream.Subscribe(_historySubject);
            relay.ConnectionStream.Subscribe(x => OnRelayConnection(x, relay));
            _relays.Add(relay);
            return relay;
        }

        private void InitRelaysToSelect()
        {
            _relaysToSelect.AddRange(DefaultRelays);
            var storedRelays = _localStorage.GetItem<string[]?>(RelaysToSelectKey);
            if (storedRelays == null)
                return;
            _relaysToSelect.AddRange(storedRelays);
        }
        
        private void InitConnectedRelays()
        {
            var connectedRelays = _localStorage.GetItem<string[]?>(RelaysConnectedKey);
            if (connectedRelays == null)
            {
                AddNextRelay();
                return;
            }

            foreach (var connectedRelayUrl in connectedRelays)
            {
                var relay = CreateRelay(connectedRelayUrl);
                ConnectPure(relay);
                _ = relay.Connect(null);
            }
            
            AddNextRelay();
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

        private void OnRelayConnection(bool connected, RelayConnection relay)
        {
            var connectedRelays = (_localStorage.GetItem<string[]?>(RelaysConnectedKey) ?? Array.Empty<string>())
                .Select(x => x.TrimEnd('/'))
                .ToHashSet();
            var relayUrl = relay.RelayUrl.ToString().TrimEnd('/');
            if (connected)
                connectedRelays.Add(relayUrl);
            else
                connectedRelays.Remove(relayUrl);
            _localStorage.SetItem(RelaysConnectedKey, connectedRelays.ToArray());
            _connectionSubject.OnNext(connected);
        }
    }
}
