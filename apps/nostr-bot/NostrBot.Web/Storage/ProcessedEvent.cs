using Nostr.Client.Messages;

namespace NostrBot.Web.Storage
{
    public class ProcessedEvent
    {
        public long ProcessedEventId { get; set; }

        public DateTime Created { get; set; }

        public string? Subscription { get; set; }

        public string Relay { get; set; } = null!;

        public string? NostrEventId { get; set; }

        public string? NostrEventContent { get; set; }

        public string? NostrEventPubkey { get; set; }

        public NostrKind NostrEventKind { get; set; }

        public DateTime? NostrEventCreatedAt { get; set; }

        public string? NostrEventTagP { get; set; }

        public string? NostrEventTagE { get; set; }

        public string ReplyContextId { get; set; } = null!;

        public string? ReplySecondaryContextId { get; set; }

        public string? GeneratedReply { get; set; }
    }
}
