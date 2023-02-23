using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Nostr.Client.Communicator;
using Websocket.Client;
using Websocket.Client.Models;

namespace Nostr.Client.Tests
{
    public class NostrFakeCommunicator : INostrCommunicator
    {
        private readonly Subject<ResponseMessage> _messageSubject = new();
        private readonly List<string> _sentMessages = new();

        public IReadOnlyCollection<string> SentMessages => _sentMessages;

        public void Dispose()
        {
            IsStarted = false;
            IsRunning = false;
        }

        public Task Start()
        {
            IsStarted = true;
            IsRunning = true;
            return Task.CompletedTask;
        }

        public Task StartOrFail()
        {
            IsStarted = true;
            IsRunning = true;
            return Task.CompletedTask;
        }

        public Task<bool> Stop(WebSocketCloseStatus status, string statusDescription)
        {
            IsStarted = false;
            IsRunning = false;
            return Task.FromResult(true);
        }

        public Task<bool> StopOrFail(WebSocketCloseStatus status, string statusDescription)
        {
            IsStarted = false;
            IsRunning = false;
            return Task.FromResult(true);
        }

        public void Send(string message)
        {
            _sentMessages.Add(message);
        }

        public void Send(byte[] message)
        {
            throw new NotImplementedException();
        }

        public void Send(ArraySegment<byte> message)
        {
            throw new NotImplementedException();
        }

        public Task SendInstant(string message)
        {
            return Task.CompletedTask;
        }

        public Task SendInstant(byte[] message)
        {
            return Task.CompletedTask;
        }

        public Task Reconnect()
        {
            return Task.CompletedTask;
        }

        public Task ReconnectOrFail()
        {
            return Task.CompletedTask;
        }

        public void StreamFakeMessage(ResponseMessage message)
        {
        }

        public Uri Url { get; set; } = new("wss://fake.local");
        public IObservable<ResponseMessage> MessageReceived => _messageSubject.AsObservable();
        public IObservable<ReconnectionInfo> ReconnectionHappened => Observable.Empty<ReconnectionInfo>();
        public IObservable<DisconnectionInfo> DisconnectionHappened => Observable.Empty<DisconnectionInfo>();
        public TimeSpan? ReconnectTimeout { get; set; }
        public TimeSpan? ErrorReconnectTimeout { get; set; }
        public string Name { get; set; } = "fake";
        public bool IsStarted { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsReconnectionEnabled { get; set; }
        public bool IsTextMessageConversionEnabled { get; set; }
        public ClientWebSocket NativeClient { get; }
        public Encoding MessageEncoding { get; set; } = Encoding.UTF8;
    }
}
