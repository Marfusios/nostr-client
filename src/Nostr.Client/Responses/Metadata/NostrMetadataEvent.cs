using Nostr.Client.Json;

namespace Nostr.Client.Responses.Metadata
{
    public class NostrMetadataEvent : NostrEvent
    {
        public NostrMetadataEvent(string? content)
        {
            Content = content;
            Metadata = NostrSerializer.DeserializeSafely<NostrMetadata>(content);
        }

        public NostrMetadata? Metadata { get; init; }
    }
}
