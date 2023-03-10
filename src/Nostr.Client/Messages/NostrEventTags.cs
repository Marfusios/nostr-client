using System.Collections.ObjectModel;

namespace Nostr.Client.Messages
{
    public class NostrEventTags : Collection<NostrEventTag>
    {
        public static NostrEventTags Empty => new();

        public NostrEventTags()
        {
        }

        public NostrEventTags(params NostrEventTag[] tags)
        {
            foreach (var tag in tags)
            {
                Add(tag);
            }
        }

        public NostrEventTag[] GetEvents()
        {
            return Get(NostrEventTag.EventIdentifier);
        }

        public NostrEventTag[] GetProfiles()
        {
            return Get(NostrEventTag.ProfileIdentifier);
        }

        public NostrEventTag[] Get(string? tagIdentifier)
        {
            return this.Where(x => x.TagIdentifier == tagIdentifier).ToArray();
        }

        public NostrEventTag? FindFirstTag(string? tagIdentifier)
        {
            var tags = Get(tagIdentifier);
            return tags.FirstOrDefault();
        }

        public string? FindFirstTagValue(string? tagIdentifier)
        {
            var first = FindFirstTag(tagIdentifier);
            return first?.AdditionalData?.FirstOrDefault()?.ToString();
        }

        public bool ContainsEvent(string? eventId)
        {
            return ContainsTag(NostrEventTag.EventIdentifier, eventId);
        }

        public bool ContainsProfile(string? pubkey)
        {
            return ContainsTag(NostrEventTag.ProfileIdentifier, pubkey);
        }

        public bool ContainsTag(string? tagIdentifier, string? tagValue)
        {
            var tags = Get(tagIdentifier);
            return tags.Any(x => x.AdditionalData.Any(y => y?.ToString() == tagValue));
        }

        public NostrEventTags DeepClone()
        {
            var clone = new NostrEventTags();
            foreach (var tag in this)
            {
                clone.Add(tag.DeepClone());
            }
            return clone;
        }
    }
}
