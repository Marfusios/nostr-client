using Microsoft.Extensions.Options;
using Nostr.Client.Client;
using Nostr.Client.Identifiers;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Utils;
using NostrBot.Web.Configs;
using OpenAI;
using OpenAI.Images;

namespace NostrBot.Web.Logic
{
    public class BotManagement
    {
        private const char CommandPrefix = '!';
        private const string CommandReplyTo = "reply";
        private const string CommandGenerateImage = "image";

        private readonly BotConfig _config;
        private readonly OpenAiConfig _aiConfig;
        private readonly NostrMultiWebsocketClient _client;
        private readonly OpenAIClient _aiClient;

        public BotManagement(IOptions<BotConfig> config, IOptions<OpenAiConfig> aiConfig,
            NostrMultiWebsocketClient client, OpenAIClient aiClient)
        {
            _config = config.Value;
            _aiConfig = aiConfig.Value;
            _client = client;
            _aiClient = aiClient;
        }

        public bool IsCommand(string? message)
        {
            var messageSafe = message ?? string.Empty;
            return messageSafe.StartsWith(CommandPrefix);
        }

        public async Task<string> ProcessCommand(string? message, string? senderPubKey)
        {
            var messageSafe = (message ?? string.Empty).TrimStart(CommandPrefix);

            if (string.IsNullOrWhiteSpace(messageSafe))
                return "Received message is empty, cannot continue";

            if (string.IsNullOrWhiteSpace(senderPubKey))
                return "Sender pubkey is not specified, cannot process command";

            var senderKey = NostrPublicKey.FromHex(senderPubKey);
            if (!_config.BotAdminPubKeys.Contains(senderKey.Bech32) && !_config.BotAdminPubKeys.Contains(senderKey.Hex))
                return "Sender is not admin, ignore command";

            var targetCommand = messageSafe.ToLowerInvariant();
            if (targetCommand.StartsWith(CommandReplyTo))
            {
                return OnReply(messageSafe);
            }

            if (targetCommand.StartsWith(CommandGenerateImage))
            {
                return await OnImage(messageSafe);
            }

            return "Unknown command";
        }

        private string OnReply(string messageSafe)
        {
            var split = messageSafe.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                return "Invalid command format";

            var content = split[1];

            // try to parse 'nevent1'
            if (NostrIdentifierParser.TryParse(content, out var identifier) &&
                identifier is NostrEventIdentifier identifierEvent)
            {
                content = identifierEvent.EventId;
            }

            // parse 'note1' into hex
            if (NostrConverter.TryToHex(content, out var targetEventIdHex, out _))
                content = targetEventIdHex!;

            var filter = new NostrFilter
            {
                Kinds = new[]
                {
                    NostrKind.ShortTextNote,
                    NostrKind.EncryptedDm
                },
                Ids = new[] { content }
            };
            _client.Send(new NostrRequest(content, filter));
            return $"Requesting event {content}";
        }
        
        private async Task<string> OnImage(string messageSafe)
        {
            var commandLenght = CommandGenerateImage.Length;
            var content = messageSafe[commandLenght..].Trim();

            var request = new ImageGenerationRequest(
                content,
                _aiConfig.ImageModel,
                _aiConfig.ImageCount,
                quality: _aiConfig.ImageQuality
                );
            var response = await _aiClient.ImagesEndPoint.GenerateImageAsync(request);

            return string.Join(" ", response.Select(x => $"{x.RevisedPrompt} \n\n{x.Url}"));
        }
    }
}
