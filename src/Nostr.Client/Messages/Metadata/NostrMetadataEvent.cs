using Newtonsoft.Json;
using Nostr.Client.Json;

namespace Nostr.Client.Messages.Metadata
{
    public class NostrMetadataEvent : NostrEvent
    {
        public NostrMetadataEvent(string? content)
        {
            Content = content;
            Metadata = NostrJson.DeserializeSafely<NostrMetadata>(content);
        }

        [JsonIgnore]
        public NostrMetadata? Metadata { get; init; }
    }
}
