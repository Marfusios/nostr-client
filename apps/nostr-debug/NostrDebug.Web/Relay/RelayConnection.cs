using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Nostr.Client.Communicator;
using Websocket.Client;
using Websocket.Client.Models;

namespace NostrDebug.Web.Relay
{
    public class RelayConnection : IDisposable
    {
        private readonly ILogger<RelayConnection> _logger;

        private readonly NostrWebsocketCommunicator _communicator;

        private readonly Subject<string> _historySubject = new();
        private readonly Subject<bool> _connectionSubject = new();

        public RelayConnection(ILogger<RelayConnection> logger, Uri url)
        {
            _logger = logger;

            _communicator = new NostrWebsocketCommunicator(url);

            _communicator.Name = url.Host;
            _communicator.ReconnectTimeout = null; //TimeSpan.FromSeconds(30);
            _communicator.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

            _communicator.ReconnectionHappened.Subscribe(OnReconnected);
            _communicator.DisconnectionHappened.Subscribe(OnDisconnected);
            _communicator.MessageReceived.Subscribe(OnMessageReceived);
        }

        public INostrCommunicator Communicator => _communicator;

        public bool IsConnecting => _communicator.IsStarted;
        public bool IsConnected => _communicator.IsRunning;
        public bool IsStarted => _communicator.IsStarted;

        public bool IsUsed { get; private set; }

        public Uri RelayUrl => _communicator.Url;

        public int ReceivedMessagesCount { get; private set; }

        public IObservable<string> HistoryStream => _historySubject.AsObservable();
        public IObservable<bool> ConnectionStream => _connectionSubject.AsObservable();

        public void Dispose()
        {
            _communicator.Dispose();
        }

        public async Task<bool> Connect(string relayUrl)
        {
            IsUsed = true;
            if (_communicator.IsRunning)
            {
                return false;
            }

            if (!Uri.TryCreate(relayUrl, UriKind.Absolute, out var safeUrl))
            {
                return false;
            }

            ReceivedMessagesCount = 0;
            _communicator.Url = safeUrl;
            _communicator.Name = safeUrl.Host;
            _ = _communicator.Start();
            return true;
        }

        public async Task Disconnect()
        {
            await _communicator.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
        }

        private void OnMessageReceived(ResponseMessage message)
        {
            ReceivedMessagesCount++;
        }

        private void OnReconnected(ReconnectionInfo info)
        {
            var subMessage = info.Type switch
            {
                ReconnectionType.Initial => "Connected",
                _ => $"Reconnected, type: {info.Type}"
            };
            var message = $"[{DateTime.Now:HH:mm:ss.fff} {_communicator.Name}] ✅ {subMessage}";
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
                $"[{DateTime.Now:HH:mm:ss.fff} {_communicator.Name}] ❌ Disconnected, type: {info.Type}{reason}";
            _logger.LogInformation(message);
            _historySubject.OnNext(message);
            _connectionSubject.OnNext(false);
        }
    }
}
