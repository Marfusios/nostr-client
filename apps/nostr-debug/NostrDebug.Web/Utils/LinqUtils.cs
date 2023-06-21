namespace NostrDebug.Web.Utils
{
    public static class LinqUtils
    {
        public static void AddRange<T>(this HashSet<T> collection, IEnumerable<T> toAdd)
        {
            foreach (var item in toAdd)
            {
                collection.Add(item);
            }
        }
    }
}
