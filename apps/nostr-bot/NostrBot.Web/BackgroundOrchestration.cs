using Microsoft.Extensions.Options;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Responses;
using NostrBot.Web.Configs;
using NostrBot.Web.Logic;
using Serilog;

namespace NostrBot.Web
{
    public class BackgroundOrchestration : IHostedService
    {
        private readonly NostrConfig _nostrConfig;
        private readonly NostrListener _listener;
        private readonly NostrEventsQueue _eventsQueue;

        private IDisposable? _eventsSub;

        public BackgroundOrchestration(IOptions<NostrConfig> nostrConfig, NostrListener listener, NostrEventsQueue eventsQueue)
        {
            _listener = listener;
            _eventsQueue = eventsQueue;
            _nostrConfig = nostrConfig.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Starting Nostr Bot");

            var botPubKey = NostrPrivateKey.FromBech32(_nostrConfig.PrivateKey).DerivePublicKey();
            Log.Information("Bot public key: {pubkey}", botPubKey.Bech32);

            _listener.RegisterFilter(new NostrFilter
            {
                Kinds = new[]
                {
                    NostrKind.ShortTextNote,
                    NostrKind.EncryptedDm
                },
                P = new[] { botPubKey.Hex }
            });

            _eventsSub = _listener.Streams.EventStream.Subscribe(OnEvent);

            _listener.Start();

            Log.Information("Nostr Bot listening...");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Stop();
            _eventsQueue.Writer.TryComplete();
            _eventsSub?.Dispose();

            return Task.CompletedTask;
        }

        private void OnEvent(NostrEventResponse response)
        {
            _eventsQueue.Writer.TryWrite(response);
        }
    }
}
