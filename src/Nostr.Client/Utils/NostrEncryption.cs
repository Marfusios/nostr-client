using System.Security.Cryptography;
using Nostr.Client.Keys;

namespace Nostr.Client.Utils
{
    /// <summary>
    /// Nostr encryption utilities
    /// </summary>
    public static class NostrEncryption
    {
        public static EncryptedData Encrypt(byte[] plainText, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;

            var encrypted = aes.EncryptCbc(plainText, aes.IV);
            return new EncryptedData(encrypted, aes.IV);
        }

        public static EncryptedBase64Data EncryptBase64(byte[] plainText, byte[] key)
        {
            var encrypted = Encrypt(plainText, key);
            return new EncryptedBase64Data(
                Convert.ToBase64String(encrypted.Text),
                Convert.ToBase64String(encrypted.Iv)
            );
        }

        public static EncryptedBase64Data EncryptBase64(byte[] plainText, NostrPublicKey key)
        {
            return EncryptBase64(plainText, key.Ec.ToBytes().ToArray());
        }

        public static byte[] Decrypt(EncryptedData encrypted, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;

            return aes.DecryptCbc(encrypted.Text, encrypted.Iv);
        }

        public static byte[] DecryptBase64(EncryptedBase64Data encrypted, byte[] key)
        {
            var textBytes = Convert.FromBase64String(encrypted.Text);
            var ivBytes = Convert.FromBase64String(encrypted.Iv);

            var decrypted = Decrypt(new EncryptedData(textBytes, ivBytes), key);
            return decrypted;
        }

        public static byte[] DecryptBase64(EncryptedBase64Data encrypted, NostrPublicKey key)
        {
            return DecryptBase64(encrypted, key.Ec.ToBytes().ToArray());
        }
    }

    public record EncryptedData(byte[] Text, byte[] Iv);

    public record EncryptedBase64Data(string Text, string Iv);
}
