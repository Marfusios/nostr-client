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
        private readonly BotManagement _management;

        public BotMind(IOptions<NostrConfig> nostrConfig, IOptions<BotConfig> config,
            NostrMultiWebsocketClient client, NostrEventsQueue eventsQueue, OpenAIClient openAi, BotStorage storage, BotManagement management)
        {
            _eventsQueue = eventsQueue;
            _openAi = openAi;
            _storage = storage;
            _management = management;
            _nostrConfig = nostrConfig.Value;
            _config = config.Value;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Bot description: {description}", _config.BotDescription);
            Log.Information("Bot whois: {description}", _config.BotWhois);

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

            var contextId = GenerateContextIdForPubkey(ev);
            var secondaryContextId = GenerateContextIdForReplyOrRoot(ev);
            var aiReply = await RequestAiReply(response, contextId, secondaryContextId, message);

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

            var newSecondaryContextId = GenerateContextIdForRoot(signed);
            await _storage.Store(contextId, response, ev, aiReply, message, newSecondaryContextId);
        }

        private async Task OnDirectMessage(NostrEventResponse response, NostrEncryptedDirectEvent dm)
        {
            var botKey = NostrPrivateKey.FromBech32(_nostrConfig.PrivateKey);
            var decryptedMessage = dm.DecryptContent(botKey);

            var receiver = NostrPublicKey.FromHex(dm.Pubkey ?? throw new InvalidOperationException("DM pubkey is null"));

            if (_management.IsCommand(decryptedMessage))
            {
                Log.Debug("[{relay}] Received dm command, content: {message}, processing...", response.CommunicatorName, decryptedMessage);
                var comment = await _management.ProcessCommand(decryptedMessage, dm.Pubkey);
                SendDirectMessage(comment, botKey, receiver);
                await _storage.Store("command", response, dm, comment, decryptedMessage, null);
                return;
            }

            Log.Debug("[{relay}] Received dm, message: {message}, generating AI reply...", response.CommunicatorName, decryptedMessage);

            var contextId = GenerateContextIdForPubkey(dm);
            var aiReply = await RequestAiReply(response, contextId, null, decryptedMessage);

            SendDirectMessage(aiReply, botKey, receiver);
            await _storage.Store(contextId, response, dm, aiReply, decryptedMessage, null);
        }

        private void SendDirectMessage(string message, NostrPrivateKey sender, NostrPublicKey receiver)
        {
            var replyDm = new NostrEvent
            {
                Kind = NostrKind.EncryptedDm,
                CreatedAt = DateTime.UtcNow,
                Content = message
            };
            var encrypted = replyDm.EncryptDirect(sender, receiver);
            var signed = encrypted.Sign(sender);
            _client.Send(new NostrEventRequest(signed));
        }

        private async Task<string> RequestAiReply(NostrEventResponse response, string contextId, string? secondaryContextId, string? userMessage)
        {
            var chatPrompts = new List<ChatPrompt>();
            chatPrompts.AddRange(IncludeBotDescription());
            chatPrompts.AddRange(IncludeBotWhois());
            chatPrompts.AddRange(await IncludeHistory(contextId, secondaryContextId));
            chatPrompts.Add(new ChatPrompt("user", $"{response.Event?.Pubkey}: {userMessage}"));

            var chatRequest = new ChatRequest(chatPrompts);
            var result = await _openAi.ChatEndpoint.GetCompletionAsync(chatRequest);

            var aiReply = string.Join(Environment.NewLine, result.Choices.Select(x => x.Message.Content));

            Log.Debug("[{relay}] AI reply to message: {message}, reply: {reply}", response.CommunicatorName, userMessage,
                aiReply);
            return aiReply;
        }

        private string GenerateContextIdForPubkey(string? pubkey)
        {
            return $"mention-from-{pubkey}";
        }

        private string GenerateContextIdForPubkey(NostrEvent ev)
        {
            return GenerateContextIdForPubkey(ev.Pubkey);
        }

        private string GenerateContextIdForReplyOrRoot(NostrEvent ev)
        {
            var fromTag = ev.Tags?.FindFirstTagValue(NostrEventTag.EventIdentifier);
            return $"mention-id-{fromTag ?? ev.Id}";
        }

        private string GenerateContextIdForRoot(NostrEvent ev)
        {
            return $"mention-id-{ev.Id}";
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
                new ChatPrompt("user", "unknown: Who are you?"),
                new ChatPrompt("assistant", _config.BotWhois)
            };
        }

        private async Task<IEnumerable<ChatPrompt>> IncludeHistory(string contextId, string? secondaryContextId)
        {
            var historicalEvents = (await _storage.GetHistoryForContext(contextId, secondaryContextId)).ToList();
            if (!historicalEvents.Any())
            {
                return Array.Empty<ChatPrompt>();
            }

            foreach (var historicalEvent in historicalEvents.ToArray())
            {
                await LoadAdditionalHistory(historicalEvent, historicalEvents);
            }

            var prompts = new List<ChatPromptTimed>();
            var maxSize = 2000;
            var currentSize = 0;

            var orderedBackward = historicalEvents
                .DistinctBy(x => x.NostrEventId)
                .OrderByDescending(x => x.NostrEventCreatedAt ?? x.Created)
                .ToArray();

            foreach (var ev in orderedBackward)
            {
                var timestamp = ev.NostrEventCreatedAt ?? ev.Created;
                var request = $"{ev.NostrEventPubkey}: {ev.NostrEventContent}";
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

        private async Task LoadAdditionalHistory(ProcessedEvent ev, List<ProcessedEvent> events)
        {
            var context = GenerateContextIdForPubkey(ev.NostrEventPubkey);
            events.AddRange(await _storage.GetHistoryForContext(context, null));

            var contextRef = GenerateContextIdForPubkey(ev.NostrEventTagP);
            events.AddRange(await _storage.GetHistoryForContext(contextRef, null));
        }

        private int CountTextTokens(string text)
        {
            return text.Count(x => x == ' ');
        }

        private record ChatPromptTimed(DateTime Timestamp, ChatPrompt Prompt);
    }
}
