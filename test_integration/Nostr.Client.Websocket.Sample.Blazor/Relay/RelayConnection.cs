using System.Net.WebSockets;
using Nostr.Client.Websocket.Client;
using Nostr.Client.Websocket.Communicator;
using Websocket.Client;

namespace Nostr.Client.Websocket.Sample.Blazor.Relay
{
    public class RelayConnection : IDisposable
    {
        public static string[] DefaultRelays = {
            "wss://relay.snort.social",
            "wss://relay.damus.io",
            "wss://nostr-pub.wellorder.net"
        };

        private readonly ILogger<RelayConnection> _logger;
        private readonly ILogger<NostrWebsocketClient> _loggerClient;

        private readonly NostrWebsocketCommunicator _communicator;
        private readonly NostrWebsocketClient _client;

        public RelayConnection(ILogger<RelayConnection> logger, ILogger<NostrWebsocketClient> loggerClient)
        {
            _logger = logger;
            _loggerClient = loggerClient;

            var url = new Uri(DefaultRelays.First());
            _communicator = new NostrWebsocketCommunicator(url);

            _communicator.Name = $"Relay-{url.Host}";
            _communicator.ReconnectTimeout = null; //TimeSpan.FromSeconds(30);
            _communicator.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

            _communicator.ReconnectionHappened.Subscribe(info =>
                _logger.LogInformation($"[{_communicator.Url.Host}] Reconnected, type: {info.Type}"));
            _communicator.DisconnectionHappened.Subscribe(info =>
                _logger.LogInformation($"[{_communicator.Url.Host}] Disconnected, type: {info.Type}, reason: {info.CloseStatusDescription}"));

            _client = new NostrWebsocketClient(_communicator, _loggerClient);
        }

        public IWebsocketClient Communicator => _communicator;

        public NostrWebsocketClient Client => _client;

        public bool IsConnected => _communicator.IsRunning;

        public void Dispose()
        {
            _communicator.Dispose();
            _client.Dispose();
        }

        public async Task Connect(string relayUrl)
        {
            if (_communicator.IsRunning)
            {
                return;
            }

            _communicator.Url = new Uri(relayUrl);
            await _communicator.Start();
        }

        public async Task Disconnect()
        {
            if (!_communicator.IsRunning)
            {
                return;
            }

            await _communicator.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
        }
    }
}
