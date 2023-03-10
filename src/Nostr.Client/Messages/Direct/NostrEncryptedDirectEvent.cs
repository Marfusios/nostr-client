using Newtonsoft.Json;
using Nostr.Client.Keys;
using Nostr.Client.Utils;

namespace Nostr.Client.Messages.Direct
{
    public class NostrEncryptedDirectEvent : NostrEvent
    {
        private const string IvSeparator = "?iv=";

        public NostrEncryptedDirectEvent(string? content, NostrEventTags? tags)
        {
            Content = content;
            TryExtractContent(content);

            Tags = tags;
            RecipientPubkey = tags?.FindFirstTagValue(NostrEventTag.ProfileIdentifier);
        }

        [JsonIgnore]
        public string? EncryptedContent { get; private set; }

        [JsonIgnore]
        public string? InitializationVector { get; private set; }

        [JsonIgnore]
        public string? RecipientPubkey { get; private set; }

        public string? DecryptContent(NostrPrivateKey privateKey)
        {
            if (EncryptedContent == null)
                throw new InvalidOperationException("Encrypted content is null, can't decrypt");
            if (InitializationVector == null)
                throw new InvalidOperationException("Initialization vector is null, can't decrypt");
            if (RecipientPubkey == null)
                throw new InvalidOperationException("Recipient pubkey is not specified, can't decrypt");
            if (Pubkey == null)
                throw new InvalidOperationException("Sender pubkey is not specified, can't decrypt");

            var givenPubkey = privateKey.DerivePublicKey();
            string targetPubkeyHex;
            if (Pubkey == givenPubkey.Hex)
                // given is sender, use recipient pubkey
                targetPubkeyHex = RecipientPubkey;
            else if (RecipientPubkey == givenPubkey.Hex)
                // given is recipient, use sender pubkey
                targetPubkeyHex = Pubkey;
            else
                throw new InvalidOperationException(
                    "The encrypted event is not for the given private key. Sender or receiver pubkey doesn't match");

            var targetPubkey = NostrPublicKey.FromHex(targetPubkeyHex);
            var sharedKey = privateKey.DeriveSharedKey(targetPubkey);

            var encrypted = new EncryptedBase64Data(EncryptedContent, InitializationVector);
            var decrypted = NostrEncryption.DecryptBase64(encrypted, sharedKey);
            var decryptedText = HashExtensions.ToString(decrypted);
            return decryptedText;
        }

        public static NostrEncryptedDirectEvent Encrypt(NostrEvent ev, NostrPrivateKey sender)
        {
            var recipientPubkeyHex = ev.Tags?.FindFirstTagValue(NostrEventTag.ProfileIdentifier);
            if (recipientPubkeyHex == null)
                throw new InvalidOperationException("Recipient pubkey is not specified, can't encrypt");

            var recipientPubkey = NostrPublicKey.FromHex(recipientPubkeyHex);
            var sharedKey = sender.DeriveSharedKey(recipientPubkey);

            var plainText = HashExtensions.FromString(ev.Content ?? string.Empty);
            var encrypted = NostrEncryption.EncryptBase64(plainText, sharedKey);
            var encryptedContent = $"{encrypted.Text}{IvSeparator}{encrypted.Iv}";

            return new NostrEncryptedDirectEvent(encryptedContent, ev.Tags)
            {
                Kind = NostrKind.EncryptedDm,
                Pubkey = sender.DerivePublicKey().Hex,
                CreatedAt = ev.CreatedAt,
                Content = encryptedContent
            };
        }

        private void TryExtractContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var split = content.Split(IvSeparator);
            if (split.Length < 1)
            {
                EncryptedContent = split[0];
                return;
            }

            EncryptedContent = split[0];
            InitializationVector = split[1];
        }
    }
}
