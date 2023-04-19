namespace Nostr.Client.Messages.Mutable
{
    public class NostrEventMutable
    {
        /// <summary>
        /// 32-bytes lowercase hex-encoded sha256 of the the serialized event data
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 32-bytes lowercase hex-encoded public key of the event creator
        /// </summary>
        public string? Pubkey { get; set; }

        public DateTime? CreatedAt { get; set; }

        public NostrKind Kind { get; set; }

        public NostrEventTagsMutable? Tags { get; set; } = new();

        /// <summary>
        /// Arbitrary string
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// 64-bytes hex of the signature of the sha256 hash of the serialized event data, which is the same as the "id" field
        /// </summary>
        public string? Sig { get; set; }

        /// <summary>
        /// Additional unparsed data
        /// </summary>
        public Dictionary<string, object?> AdditionalData { get; set; } = new();

        public NostrEvent ToEvent()
        {
            return new NostrEvent
            {
                Id = Id,
                Pubkey = Pubkey,
                CreatedAt = CreatedAt,
                Kind = Kind,
                Tags = Tags == null ? null : new NostrEventTags(Tags.ToArray()),
                AdditionalData = AdditionalData,
                Content = Content,
                Sig = Sig
            };
        }

        public static NostrEventMutable FromEvent(NostrEvent ev)
        {
            return new NostrEventMutable
            {
                Id = ev.Id,
                Pubkey = ev.Pubkey,
                CreatedAt = ev.CreatedAt,
                Kind = ev.Kind,
                Tags = ev.Tags != null ? new NostrEventTagsMutable(ev.Tags) : null,
                AdditionalData = ev.AdditionalData as Dictionary<string, object?> ??
                                 ev.AdditionalData.ToDictionary(x => x.Key, x => x.Value),
                Content = ev.Content,
                Sig = ev.Sig
            };
        }
    }
}
