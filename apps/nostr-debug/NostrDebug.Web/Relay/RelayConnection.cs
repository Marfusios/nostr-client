using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Nostr.Client.Client;
using Nostr.Client.Communicator;
using Websocket.Client;
using Websocket.Client.Models;

namespace NostrDebug.Web.Relay
{
    public class RelayConnection : IDisposable
    {
        public static readonly HashSet<string> DefaultRelays = new(new[]{
            "wss://relay.snort.social",
            "wss://relay.damus.io",
            "wss://nostr-pub.wellorder.net",
            "wss://nos.lol",
            "wss://nostr.wine",
            "wss://brb.io"
        });

        private readonly ILogger<RelayConnection> _logger;

        private readonly NostrWebsocketCommunicator _communicator;
        private readonly NostrWebsocketClient _client;

        private readonly ReplaySubject<string> _historySubject = new(5000);
        private readonly Subject<bool> _connectionSubject = new();

        public RelayConnection(ILogger<RelayConnection> logger, ILogger<NostrWebsocketClient> loggerClient)
        {
            _logger = logger;

            var url = new Uri(DefaultRelays.First());
            _communicator = new NostrWebsocketCommunicator(url);

            _communicator.Name = $"Relay-{url.Host}";
            _communicator.ReconnectTimeout = null; //TimeSpan.FromSeconds(30);
            _communicator.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

            _communicator.ReconnectionHappened.Subscribe(OnReconnected);
            _communicator.DisconnectionHappened.Subscribe(OnDisconnected);

            _client = new NostrWebsocketClient(_communicator, loggerClient);
        }

        public IWebsocketClient Communicator => _communicator;

        public NostrWebsocketClient Client => _client;

        public bool IsConnecting => _communicator.IsStarted;

        public bool IsConnected => _communicator.IsRunning;

        public string RelayUrl => _communicator.Url.ToString().TrimEnd('/');

        public IObservable<string> HistoryStream => _historySubject.AsObservable();
        public IObservable<bool> ConnectionStream => _connectionSubject.AsObservable();

        public void Dispose()
        {
            _communicator.Dispose();
            _client.Dispose();
        }

        public async Task<bool> Connect(string relayUrl)
        {
            if (_communicator.IsRunning)
            {
                return false;
            }

            if (!Uri.TryCreate(relayUrl, UriKind.Absolute, out var safeUrl))
            {
                return false;
            }

            _communicator.Url = safeUrl;
            await _communicator.Start();
            return true;
        }

        public async Task Disconnect()
        {
            await _communicator.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
        }

        private void OnReconnected(ReconnectionInfo info)
        {
            DefaultRelays.Add(RelayUrl);

            var subMessage = info.Type switch
            {
                ReconnectionType.Initial => "Connected",
                _ => $"Reconnected, type: {info.Type}"
            };
            var message = $"[{DateTime.Now:HH:mm:ss.fff} {_communicator.Url.Host}] ✅ {subMessage}";
            _logger.LogInformation(message);
            _historySubject.OnNext(message);
            _connectionSubject.OnNext(true);
        }

        private void OnDisconnected(DisconnectionInfo info)
        {
            var reason = string.IsNullOrWhiteSpace(info.CloseStatusDescription)
                ? string.Empty
                : $", reason: {info.CloseStatusDescription}";
            var message =
                $"[{DateTime.Now:HH:mm:ss.fff} {_communicator.Url.Host}] ❌ Disconnected, type: {info.Type}{reason}";
            _logger.LogInformation(message);
            _historySubject.OnNext(message);
            _connectionSubject.OnNext(false);
        }
    }
}
