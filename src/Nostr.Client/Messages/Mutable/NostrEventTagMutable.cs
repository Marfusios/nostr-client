using System.Diagnostics;

namespace Nostr.Client.Messages.Mutable
{
    [DebuggerDisplay("TagMutable {TagIdentifier} additional: {AdditionalData.Length}")]
    public class NostrEventTagMutable
    {
        public NostrEventTagMutable()
        {
        }

        public NostrEventTagMutable(string? identifier, params object[] data)
        {
            TagIdentifier = identifier;
            AdditionalData = data;
        }

        public string? TagIdentifier { get; set; }

        public object[] AdditionalData { get; set; } = Array.Empty<object>();

        public NostrEventTag ToTag()
        {
            return new NostrEventTag(TagIdentifier, AdditionalData);
        }

        public static NostrEventTagMutable FromTag(NostrEventTag tag)
        {
            return new NostrEventTagMutable
            {
                TagIdentifier = tag.TagIdentifier,
                AdditionalData = tag.AdditionalData
            };
        }
    }
}
