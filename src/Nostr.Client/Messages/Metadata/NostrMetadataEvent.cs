using Newtonsoft.Json;
using Nostr.Client.Json;

namespace Nostr.Client.Messages.Metadata
{
    public class NostrMetadataEvent : NostrEvent
    {
        public NostrMetadataEvent(string? content)
        {
            Content = content;
            Metadata = NostrSerializer.DeserializeSafely<NostrMetadata>(content);
        }

        [JsonIgnore]
        public NostrMetadata? Metadata { get; init; }
    }
}
