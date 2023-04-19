using System.Collections;
using System.Collections.ObjectModel;

namespace Nostr.Client.Messages
{
    public class NostrEventTags : IReadOnlyCollection<NostrEventTag>
    {
        public static NostrEventTags Empty => new();
        protected readonly Collection<NostrEventTag> Collection = new();

        public NostrEventTags()
        {
        }

        public NostrEventTags(IEnumerable<NostrEventTag> tags) : this(tags.ToArray())
        {
        }

        public NostrEventTags(params NostrEventTag[] tags)
        {
            foreach (var tag in tags)
            {
                Collection.Add(tag);
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

        public NostrEventTags DeepClone(params NostrEventTag[] tags)
        {
            var allTags = this.Concat(tags).ToArray();
            var clone = new NostrEventTags(allTags);
            return clone;
        }

        public IEnumerator<NostrEventTag> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Collection).GetEnumerator();
        }

        public int Count => Collection.Count;

        public NostrEventTag this[int index] => Collection[index];
    }
}
