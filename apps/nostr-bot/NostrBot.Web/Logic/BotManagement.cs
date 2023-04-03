using Microsoft.Extensions.Options;
using Nostr.Client.Client;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Utils;
using NostrBot.Web.Configs;

namespace NostrBot.Web.Logic
{
    public class BotManagement
    {
        private const char CommandPrefix = '!';
        private const string CommandReplyTo = "reply";

        private readonly BotConfig _config;
        private readonly NostrMultiWebsocketClient _client;

        public BotManagement(IOptions<BotConfig> config, NostrMultiWebsocketClient client)
        {
            _config = config.Value;
            _client = client;
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

            return "Unknown command";
        }

        private string OnReply(string messageSafe)
        {
            var split = messageSafe.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                return "Invalid command format";

            var targetEventId = split[1];
            if (NostrConverter.TryToHex(targetEventId, out var targetEventIdHex, out _))
                targetEventId = targetEventIdHex!;

            var filter = new NostrFilter
            {
                Kinds = new[]
                {
                    NostrKind.ShortTextNote,
                    NostrKind.EncryptedDm
                },
                Ids = new[] { targetEventId }
            };
            _client.Send(new NostrRequest(targetEventId, filter));
            return $"Requesting event {targetEventId}";
        }
    }
}
