namespace Nostr.Client.Messages.Mutable
{
    public class NostrEventTagsMutable : NostrEventTags, ICollection<NostrEventTag>
    {
        public NostrEventTagsMutable()
        {
        }

        public NostrEventTagsMutable(IEnumerable<NostrEventTag> collection) : base(collection)
        {
        }

        public void Add(NostrEventTag item)
        {
            Collection.Add(item);
        }

        public void Clear()
        {
            Collection.Clear();
        }

        public bool Contains(NostrEventTag item)
        {
            return Collection.Contains(item);
        }

        public void CopyTo(NostrEventTag[] array, int arrayIndex)
        {
            Collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(NostrEventTag item)
        {
            return Collection.Remove(item);
        }

        public bool IsReadOnly => false;
    }
}
