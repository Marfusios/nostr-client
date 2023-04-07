using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using Nostr.Client.Client;
using NostrBot.Web.Configs;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Messages.Direct;
using Nostr.Client.Requests;
using Nostr.Client.Responses;
using Nostr.Client.Utils;
using NostrBot.Web.Storage;
using OpenAI;
using Serilog;
using OpenAI.Chat;

namespace NostrBot.Web.Logic
{
    public class BotMind : BackgroundService
    {
        public const string MentionSubscription = "bot:mentions";
        public const string GlobalSubscription = "bot:global";
        public const string AdhocDataCollectionSubscription = "bot:adhoc";
        
        private readonly NostrConfig _nostrConfig;
        private readonly BotConfig _config;
        private readonly NostrMultiWebsocketClient _client;
        private readonly NostrEventsQueue _eventsQueue;
        private readonly OpenAIClient _openAi;
        private readonly BotStorage _storage;
        private readonly BotManagement _management;

        private readonly NostrPrivateKey _botPrivateKey;
        private readonly NostrPublicKey _botPublicKey;

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
            
            _botPrivateKey = NostrPrivateKey.FromBech32(_nostrConfig.PrivateKey);
            _botPublicKey = _botPrivateKey.DerivePublicKey();
        }

        public bool ListenToGlobalFeed => _config.ListenToGlobalFeed && _config.GlobalFeedKeywords.Any();

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

            if (ShouldIgnore(response))
                return;
            
            if (await _storage.IsProcessed(response.Event))
            {
                Log.Verbose("[{relay}] Received event is already processed, content: {content}", response.CommunicatorName, response.Event.Content);
                return;
            }

            if (response.Event is NostrEncryptedDirectEvent dm)
            {
                await OnDirectMessage(response, dm);
                return;
            }

            await OnMention(response);
        }

        private bool ShouldIgnore(NostrEventResponse response)
        {
            var subscription = response.Subscription ?? string.Empty;
            if (subscription.StartsWith(AdhocDataCollectionSubscription))
            {
                // loading adhoc thread data, ignore here
                return true;
            }
            
            var authorPubKey = ToNpub(response.Event?.Pubkey);
            if (authorPubKey == _botPublicKey.Bech32)
            {
                // ignore events from this bot
                return true;
            }

            if (_config.BotIgnoreListPubKeys.Contains(authorPubKey))
            {
                return true;
            }
            
            if (MentionSubscription.Equals(subscription, StringComparison.OrdinalIgnoreCase))
            {
                // always process direct mentions
                return false;
            }

            if (!GlobalSubscription.Equals(subscription, StringComparison.OrdinalIgnoreCase))
            {
                // process non-global stuff
                return false;
            }

            var isRoot = response.Event?.Tags?.FindFirstTagValue(NostrEventTag.EventIdentifier) == null;
            if (isRoot && !_config.ReactToRootEventsInGlobalFeed)
            {
                // ignore root events
                return true;
            }

            if (!isRoot && !_config.ReactToThreadsInGlobalFeed)
            {
                // ignore events in threads
                return true;
            }
            
            var contentSafe = (response.Event?.Content ?? string.Empty)
                .ToLowerInvariant()
                .Split(" ");
            foreach (var keyword in _config.GlobalFeedKeywords)
            {
                var keywordSafe = keyword.ToLowerInvariant();
                if (contentSafe.Contains(keywordSafe))
                    return false;
            }

            return true;
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

            var processedMessage = ExtractMentionsSafely(aiReply, out NostrEventTags tags);
            tags.Add(new NostrEventTag("e", ev.Id ?? string.Empty));
            tags.Add(new NostrEventTag("p", ev.Pubkey ?? string.Empty));
            
            var replyEvent = new NostrEvent
            {
                Kind = ev.Kind,
                CreatedAt = DateTime.UtcNow,
                Content = processedMessage,
                Tags = tags
            };

            var signed = replyEvent.Sign(botKey);
            _client.Send(new NostrEventRequest(signed));

            var newSecondaryContextId = GenerateContextIdForRoot(signed);
            await _storage.Store(contextId, response, ev, aiReply, message, newSecondaryContextId);
        }

        private async Task OnDirectMessage(NostrEventResponse response, NostrEncryptedDirectEvent dm)
        {
            var decryptedMessage = dm.DecryptContent(_botPrivateKey);

            var receiver = NostrPublicKey.FromHex(dm.Pubkey ?? throw new InvalidOperationException("DM pubkey is null"));

            if (_management.IsCommand(decryptedMessage))
            {
                Log.Debug("[{relay}] Received dm command, content: {message}, processing...", response.CommunicatorName, decryptedMessage);
                var comment = await _management.ProcessCommand(decryptedMessage, dm.Pubkey);
                SendDirectMessage(comment, _botPrivateKey, receiver);
                await _storage.Store("command", response, dm, comment, decryptedMessage, null);
                return;
            }

            Log.Debug("[{relay}] Received dm, message: {message}, generating AI reply...", response.CommunicatorName, decryptedMessage);

            var contextId = GenerateContextIdForPubkey(dm);
            var aiReply = await RequestAiReply(response, contextId, null, decryptedMessage);

            SendDirectMessage(aiReply, _botPrivateKey, receiver);
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

        private async Task<string> RequestAiReply(NostrEventResponse response, string contextId, 
            string? secondaryContextId, string? userMessage)
        {
            var userMessageProcessed = ApplyMentionsSafely(userMessage, response.Event?.Tags);
            
            var chatPrompts = new List<ChatPrompt>();
            chatPrompts.AddRange(IncludeBotDescription());
            chatPrompts.AddRange(IncludeBotWhois());
            chatPrompts.AddRange(await IncludeHistory(contextId, secondaryContextId, response));
            chatPrompts.Add(new ChatPrompt("user", $"@{ToNpub(response.Event?.Pubkey)}: {userMessageProcessed}"));

            CircuitBreaker(chatPrompts);

            var chatRequest = new ChatRequest(chatPrompts);
            var result = await _openAi.ChatEndpoint.GetCompletionAsync(chatRequest);

            var aiReply = string.Join(Environment.NewLine, result.Choices.Select(x => x.Message.Content));

            Log.Debug("[{relay}] AI reply: {reply}", response.CommunicatorName, aiReply);
            await SlowdownReply(aiReply);
            return aiReply;
        }

        private async Task SlowdownReply(string reply)
        {
            if (!_config.SlowdownReplies)
                return;

            var tokens = CountTextTokens(reply);
            var tokenPerSec = 3;
            var secondsToWait = tokens / tokenPerSec;
            Log.Debug("Slowdown enabled, waiting: {seconds} secs", secondsToWait);
            await Task.Delay(TimeSpan.FromSeconds(secondsToWait));
        }

        private void CircuitBreaker(List<ChatPrompt> chatPrompts)
        {
            var text = string.Join(Environment.NewLine, chatPrompts.Select(x => x.Content)).ToLowerInvariant();
            var textWords = text.Split(" ");
            var breakWords = new[] { "take care", "farewell", "goodbye" };
            foreach (var word in breakWords)
            {
                var occurred = textWords.Count(x => x == word);
                if (occurred > 10)
                {
                    Log.Debug("Circuit breaker triggered, word {word} occurred {occurred} times", word, occurred);
                    throw new InvalidOperationException("Circuit breaker triggered");
                }
            }
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

            var description = $"{_config.BotDescription} Your identification is {_botPublicKey.Bech32}";
            return new[]
            {
                new ChatPrompt("system", description)
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

        private async Task<IEnumerable<ChatPrompt>> IncludeHistory(string contextId, string? secondaryContextId,
            NostrEventResponse response)
        {
            var historicalEvents = (await _storage.GetHistoryForContext(contextId, secondaryContextId)).ToList();
            // var threadEvents = LoadThreadAsProcessed(response);
            // historicalEvents.AddRange(threadEvents);

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
                var request = $"@{ToNpub(ev.NostrEventPubkey)}: {ev.NostrEventContent}";
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

            if (string.IsNullOrWhiteSpace(ev.NostrEventTagP))
                return;
            
            var contextRef = GenerateContextIdForPubkey(ev.NostrEventTagP);
            events.AddRange(await _storage.GetHistoryForContext(contextRef, null));
        }

        private int CountTextTokens(string text)
        {
            return text.Count(x => x == ' ');
        }

        private string ToNpub(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return string.Empty;
            try
            {
                return NostrConverter.ToNpub(hex) ?? string.Empty;
            }
            catch (Exception)
            {
                // ignore
            }

            return hex;
        }
        
        private string? ApplyMentionsSafely(string? message, NostrEventTags? tags)
        {
            if (tags == null || !tags.Any())
                return message;
            
            try
            {
                return ApplyMentions(message, tags);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to apply mentions to message: {message}, error: {error}",
                    message, e.Message);
                return message;
            }
        }
        
        private string? ApplyMentions(string? message, NostrEventTags tags)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;
            var split = message.Split(" ");
            for (int i = 0; i < split.Length; i++)
            {
                var word = split[i];
                if(string.IsNullOrWhiteSpace(word) || !word.StartsWith("#[") || !word.EndsWith("]"))
                    continue;
                var indexStr = word.TrimStart('#').TrimStart('[').TrimEnd(']');
                if(!int.TryParse(indexStr, out int index))
                    continue;
                if(tags.Count < index)
                    continue;
                var hex = tags[index].AdditionalData.FirstOrDefault() as string;
                var npub = ToNpub(hex);
                split[i] = $"@{npub}";
            }
            return string.Join(" ", split);
        }
        
        private string? ExtractMentionsSafely(string? message, out NostrEventTags tags)
        {
            try
            {
                return ExtractMentions(message, out tags);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to extract mentions from message: {message}, error: {error}",
                    message, e.Message);
                tags = new NostrEventTags();
                return message;
            }
        }
        
        private string? ExtractMentions(string? message, out NostrEventTags tags)
        {
            tags = new NostrEventTags();
            if (string.IsNullOrWhiteSpace(message))
                return message;
            var split = message.Split(" ");
            var npubCounter = 0;
            for (int i = 0; i < split.Length; i++)
            {
                var word = split[i];
                if(string.IsNullOrWhiteSpace(word))
                    continue;
                var wordTrimmed = word
                    .Trim('.', ',', ';', '!', '\'', '"', '\\', '/',
                        '{', '}', '(', ')', '[', ']', '@', '#');
                if (!wordTrimmed.StartsWith("npub1"))
                    continue;
                var pubKey = NostrPublicKey.FromBech32(wordTrimmed);
                split[i] = $"#[{npubCounter}]";
                tags.Add(new NostrEventTag(NostrEventTag.ProfileIdentifier, pubKey.Hex));
                npubCounter++;
            }
            return string.Join(" ", split);
        }

        private ProcessedEvent[] LoadThreadAsProcessed(NostrEventResponse response)
        {
            var evRootId = response.Event?.Tags?.FindFirstTagValue(NostrEventTag.EventIdentifier);
            if (string.IsNullOrWhiteSpace(evRootId))
                return Array.Empty<ProcessedEvent>();
            
            var events = LoadFullThread(evRootId);
            return events
                .Where(x => x.Event != null)
                .Select(x => new ProcessedEvent()
                {
                    NostrEventCreatedAt = x.Event!.CreatedAt,
                    NostrEventId = x.Event!.Id,
                    NostrEventContent = x.Event!.Content,
                    NostrEventPubkey = x.Event!.Pubkey,
                    NostrEventKind = x.Event!.Kind,
                    Relay = x.CommunicatorName,
                    Created = DateTime.UtcNow
                })
                .ToArray();
        }
        
        private NostrEventResponse[] LoadFullThread(string eventIdHex)
        {
            var subscription = $"{AdhocDataCollectionSubscription}:{Guid.NewGuid():N}";
            _client.Send(new NostrRequest(subscription, new NostrFilter
            {
                Kinds = new [] { NostrKind.ShortTextNote },
                E = new []{ eventIdHex }
            }));
            var eosCounter = 0;
            var result = _client.Streams.EventStream
                    .Where(x => x.Subscription == subscription)
                    .Select(x => (NostrResponse)x)
                .Merge(_client.Streams.EoseStream.Where(x => x.Subscription == subscription))
                .Do(x => eosCounter += x is NostrEoseResponse ? 1 : 0)
                .TakeWhile(_ => eosCounter < _client.Clients.Count)
                .Timeout(TimeSpan.FromSeconds(5)).Catch(Observable.Return(default(NostrResponse)))
                .Where(x => x is not null)
                .ToArray()
                .Wait();
            return result
                .OfType<NostrEventResponse>()
                .ToArray();
        }

        private record ChatPromptTimed(DateTime Timestamp, ChatPrompt Prompt);
    }
}
