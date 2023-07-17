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
        private readonly BotMind _botMind;

        private IDisposable? _eventsSub;

        public BackgroundOrchestration(IOptions<NostrConfig> nostrConfig, NostrListener listener,
            NostrEventsQueue eventsQueue, BotMind botMind)
        {
            _listener = listener;
            _eventsQueue = eventsQueue;
            _botMind = botMind;
            _nostrConfig = nostrConfig.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Starting Nostr Bot");

            var botPubKey = NostrPrivateKey.FromBech32(_nostrConfig.PrivateKey).DerivePublicKey();
            Log.Information("Bot public key: {pubkey}", botPubKey.Bech32);

            _listener.RegisterFilter(BotMind.MentionSubscription, new NostrFilter
            {
                Kinds = new[]
                {
                    NostrKind.ShortTextNote,
                    NostrKind.EncryptedDm,
                    NostrKind.LiveChatMessage
                },
                P = new[] { botPubKey.Hex }
            });

            if (_botMind.ListenToGlobalFeed)
            {
                _listener.RegisterFilter(BotMind.GlobalSubscription, new NostrFilter
                {
                    Kinds = new[]
                    {
                        NostrKind.ShortTextNote
                    },
                    Limit = 0
                });
            }

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
