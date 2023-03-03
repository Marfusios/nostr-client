using Microsoft.Extensions.Logging;
using Nostr.Client.Communicator;

namespace Nostr.Client.Client
{
    /// <summary>
    /// Nostr client that connects to a multiple relays. 
    /// Subscribe to `Streams` to handle received messages.
    /// </summary>
    public class NostrMultiWebsocketClient : INostrClient
    {
        private readonly ILogger<NostrWebsocketClient>? _logger;
        private readonly List<NostrWebsocketClient> _clients = new();
        private readonly Dictionary<NostrWebsocketClient, List<IDisposable>> _subscriptionsPerClient = new();

        public NostrMultiWebsocketClient(ILogger<NostrWebsocketClient>? logger)
        {
            _logger = logger;
        }

        public NostrMultiWebsocketClient(ILogger<NostrWebsocketClient>? logger, params INostrCommunicator[] communicators)
        {
            _logger = logger;
            foreach (var communicator in communicators)
            {
                RegisterCommunicator(communicator);
            }
        }

        public NostrMultiWebsocketClient(ILogger<NostrWebsocketClient>? logger, params NostrWebsocketClient[] clients)
        {
            _logger = logger;
            foreach (var client in clients)
            {
                RegisterClient(client);
            }
        }

        /// <summary>
        /// Provided message streams
        /// </summary>
        public NostrClientStreams Streams { get; } = new();

        /// <summary>
        /// Send message to all communicators/relays
        /// </summary>
        public void Send<T>(T request)
        {
            foreach (var client in _clients)
            {
                client.Send(request);
            }
        }

        /// <summary>
        /// Send message to the specific communicator/relay.
        /// Return false if communicator wasn't found. 
        /// </summary>
        public bool SendTo<T>(string communicatorName, T request)
        {
            var found = FindClient(communicatorName);
            if (found == null)
                return false;
            found.Send(request);
            return true;
        }

        /// <summary>
        /// Registered clients
        /// </summary>
        public IReadOnlyCollection<NostrWebsocketClient> Clients => _clients.ToArray();

        public void Dispose()
        {
            var subs = _subscriptionsPerClient.SelectMany(x => x.Value);
            foreach (var subscription in subs)
            {
                subscription.Dispose();
            }

            foreach (var client in _clients)
            {
                client.Dispose();
            }

            _subscriptionsPerClient.Clear();
            _clients.Clear();
        }

        /// <summary>
        /// Register a new communicator and forward messages. 
        /// Given communicator won't be disposed automatically.
        /// </summary>
        public void RegisterCommunicator(INostrCommunicator communicator)
        {
            var client = new NostrWebsocketClient(communicator, _logger);
            RegisterClient(client);
        }

        /// <summary>
        /// Register a new client and forward messages. 
        /// Every client will be disposed automatically.
        /// </summary>
        public void RegisterClient(NostrWebsocketClient client)
        {
            _clients.Add(client);

            // forward all streams
            ForwardStream(client, client.Streams.EventStream, Streams.EventSubject);
            ForwardStream(client, client.Streams.NoticeStream, Streams.NoticeSubject);
            ForwardStream(client, client.Streams.EoseStream, Streams.EoseSubject);
            ForwardStream(client, client.Streams.UnknownMessageStream, Streams.UnknownMessageSubject);
            ForwardStream(client, client.Streams.UnknownRawStream, Streams.UnknownRawSubject);
        }

        /// <summary>
        /// Remove registration of the client by a given communicator name.
        /// Unsubscribe from all streams. 
        /// Returns true if client was found.
        /// </summary>
        public bool RemoveRegistration(string communicatorName)
        {
            var found = FindClient(communicatorName);
            if (found == null)
                return false;

            _clients.Remove(found);
            if (!_subscriptionsPerClient.ContainsKey(found))
                return true;

            var subs = _subscriptionsPerClient[found];
            foreach (var sub in subs)
            {
                sub.Dispose();
            }

            _subscriptionsPerClient.Remove(found);
            return true;
        }

        /// <summary>
        /// Find registered client by a given communicator name.
        /// Returns null if not found. 
        /// </summary>
        public NostrWebsocketClient? FindClient(string communicatorName)
        {
            return _clients.FirstOrDefault(x => x.Communicator.Name == communicatorName);
        }

        private void ForwardStream<T>(NostrWebsocketClient client, IObservable<T> source, IObserver<T> target)
        {
            if (!_subscriptionsPerClient.ContainsKey(client))
            {
                _subscriptionsPerClient[client] = new List<IDisposable>();
            }
            var subscriptions = _subscriptionsPerClient[client];

            var sub = source.Subscribe(target);
            subscriptions.Add(sub);
        }
    }
}
