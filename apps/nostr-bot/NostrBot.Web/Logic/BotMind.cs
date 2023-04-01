using Microsoft.Extensions.Options;
using Nostr.Client.Client;
using NostrBot.Web.Configs;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Messages.Direct;
using Nostr.Client.Requests;
using Nostr.Client.Responses;
using NostrBot.Web.Storage;
using OpenAI;
using Serilog;
using OpenAI.Chat;

namespace NostrBot.Web.Logic
{
    public class BotMind : BackgroundService
    {
        private readonly NostrConfig _nostrConfig;
        private readonly BotConfig _config;
        private readonly NostrMultiWebsocketClient _client;
        private readonly NostrEventsQueue _eventsQueue;
        private readonly OpenAIClient _openAi;
        private readonly BotStorage _storage;

        public BotMind(IOptions<NostrConfig> nostrConfig, IOptions<BotConfig> config,
            NostrMultiWebsocketClient client, NostrEventsQueue eventsQueue, OpenAIClient openAi, BotStorage storage)
        {
            _eventsQueue = eventsQueue;
            _openAi = openAi;
            _storage = storage;
            _nostrConfig = nostrConfig.Value;
            _config = config.Value;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var response in _eventsQueue.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        await OnEvent(response);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "[{relay}] Failed to process event, data: {event}, error: {error}", response.CommunicatorName,
                            response.Event, e.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Log.Error(e, "Processing events thread failed, error: {error}", e.Message);
            }

            Log.Information("Processing events finished");
        }

        private async Task OnEvent(NostrEventResponse response)
        {
            if (response.Event == null)
                return;

            if (await _storage.IsProcessed(response.Event))
            {
                Log.Debug("[{relay}] Received event is already processed, content: {content}", response.CommunicatorName, response.Event.Content);
                return;
            }

            if (response.Event is NostrEncryptedDirectEvent dm)
            {
                await OnDirectMessage(response, dm);
                return;
            }

            await OnMention(response);
        }

        private async Task OnMention(NostrEventResponse response)
        {
            var botKey = NostrPrivateKey.FromBech32(_nostrConfig.PrivateKey);
            var ev = response.Event!;
            var message = ev.Content;

            Log.Debug("[{relay}] Received mention, message: {message}, generating AI reply...", response.CommunicatorName, message);

            var contextId = GenerateContextId(ev);
            var aiReply = await RequestAiReply(response, contextId, message);

            var replyEvent = new NostrEvent
            {
                Kind = ev.Kind,
                CreatedAt = DateTime.UtcNow,
                Content = aiReply,
                Tags = new NostrEventTags(
                    new NostrEventTag("e", ev.Id ?? string.Empty),
                    new NostrEventTag("p", ev.Pubkey ?? string.Empty)
                    )
            };

            var signed = replyEvent.Sign(botKey);
            _client.Send(new NostrEventRequest(signed));
            await _storage.Store(contextId, response, ev, aiReply, message);
        }

        private async Task OnDirectMessage(NostrEventResponse response, NostrEncryptedDirectEvent dm)
        {
            var botKey = NostrPrivateKey.FromBech32(_nostrConfig.PrivateKey);
            var decryptedMessage = dm.DecryptContent(botKey);

            Log.Debug("[{relay}] Received dm, message: {message}, generating AI reply...", response.CommunicatorName, decryptedMessage);

            var contextId = GenerateContextId(dm);
            var aiReply = await RequestAiReply(response, contextId, decryptedMessage);

            var receiver = NostrPublicKey.FromHex(dm.Pubkey ?? throw new InvalidOperationException("DM pubkey is null"));
            var replyDm = new NostrEvent
            {
                Kind = NostrKind.EncryptedDm,
                CreatedAt = DateTime.UtcNow,
                Content = aiReply
            };
            var encrypted = replyDm.EncryptDirect(botKey, receiver);
            var signed = encrypted.Sign(botKey);

            _client.Send(new NostrEventRequest(signed));

            await _storage.Store(contextId, response, dm, aiReply, decryptedMessage);
        }

        private async Task<string> RequestAiReply(NostrEventResponse response, string contextId, string? userMessage)
        {
            var chatPrompts = new List<ChatPrompt>();
            chatPrompts.AddRange(IncludeBotDescription());
            chatPrompts.AddRange(IncludeBotWhois());
            chatPrompts.AddRange(await IncludeHistory(contextId));
            chatPrompts.Add(new ChatPrompt("user", userMessage));

            var chatRequest = new ChatRequest(chatPrompts);
            var result = await _openAi.ChatEndpoint.GetCompletionAsync(chatRequest);

            var aiReply = string.Join(Environment.NewLine, result.Choices.Select(x => x.Message.Content));

            Log.Debug("[{relay}] AI reply to message: {message}, reply: {reply}", response.CommunicatorName, userMessage,
                aiReply);
            return aiReply;
        }

        private string GenerateContextId(NostrEvent ev)
        {
            return $"mention-from-{ev.Pubkey}";
        }

        private string GenerateContextId(NostrEncryptedDirectEvent dm)
        {
            return $"dm-{dm.Pubkey}";
        }

        private IEnumerable<ChatPrompt> IncludeBotDescription()
        {
            if (string.IsNullOrWhiteSpace(_config.BotDescription))
            {
                return Array.Empty<ChatPrompt>();
            }
            return new[]
            {
                new ChatPrompt("system", _config.BotDescription)
            };
        }

        private IEnumerable<ChatPrompt> IncludeBotWhois()
        {
            if (string.IsNullOrWhiteSpace(_config.BotWhois))
            {
                return Array.Empty<ChatPrompt>();
            }

            return new[]
            {
                new ChatPrompt("user", "Who are you?"),
                new ChatPrompt("assistant", _config.BotWhois)
            };
        }

        private async Task<IEnumerable<ChatPrompt>> IncludeHistory(string contextId)
        {
            var historicalEvents = await _storage.GetHistoryForContext(contextId);
            if (!historicalEvents.Any())
            {
                return Array.Empty<ChatPrompt>();
            }

            var prompts = new List<ChatPromptTimed>();
            var maxSize = 3000;
            var currentSize = 0;

            var orderedBackward = historicalEvents
                .OrderByDescending(x => x.NostrEventCreatedAt ?? x.Created)
                .ToArray();

            foreach (var ev in orderedBackward)
            {
                var timestamp = ev.NostrEventCreatedAt ?? ev.Created;
                var request = ev.NostrEventContent ?? string.Empty;
                var reply = ev.GeneratedReply ?? string.Empty;

                prompts.Add(new ChatPromptTimed(timestamp, new ChatPrompt("user", request)));
                prompts.Add(new ChatPromptTimed(timestamp.AddMilliseconds(1), new ChatPrompt("assistant", reply)));

                currentSize += CountTextTokens(request) + CountTextTokens(reply);
                if (currentSize >= maxSize)
                {
                    // ignore rest of the history
                    break;
                }
            }

            var orderedPrompts = prompts
                .OrderBy(x => x.Timestamp)
                .Select(x => x.Prompt)
                .ToArray();
            return orderedPrompts;
        }

        private int CountTextTokens(string text)
        {
            return text.Count(x => x == ' ');
        }

        private record ChatPromptTimed(DateTime Timestamp, ChatPrompt Prompt);
    }
}
