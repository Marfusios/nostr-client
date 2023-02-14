using Nostr.Client.Websocket.Json;

namespace Nostr.Client.Websocket.Responses.Metadata
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
