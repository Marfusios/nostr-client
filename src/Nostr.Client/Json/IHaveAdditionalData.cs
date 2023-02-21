namespace Nostr.Client.Json
{
    /// <summary>
    /// Contains collection of additional unparsed data
    /// </summary>
    public interface IHaveAdditionalData
    {
        /// <summary>
        /// Data that wasn't parsed into properties
        /// </summary>
        public object[] AdditionalData { get; internal set; }
    }
}
