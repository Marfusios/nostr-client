using System.Diagnostics;

namespace Nostr.Client.Messages.Mutable
{
    [DebuggerDisplay("TagMutable {TagIdentifier} additional: {AdditionalData.Length}")]
    public class NostrEventTagMutable
    {
        public NostrEventTagMutable()
        {
        }

        public NostrEventTagMutable(string? identifier, params string[] data)
        {
            TagIdentifier = identifier ?? string.Empty;
            AdditionalData = data;
        }

        public string TagIdentifier { get; set; } = string.Empty;

        public string[] AdditionalData { get; set; } = Array.Empty<string>();

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
