namespace Nostr.Client.Json
{
    /// <summary>
    /// Contains collection of additional unparsed string data
    /// </summary>
    public interface IHaveAdditionalStringData
    {
        /// <summary>
        /// Data that wasn't parsed into properties
        /// </summary>
        string[] AdditionalData { get; }

        /// <summary>
        /// Set additional data, should not be used outside of this library
        /// </summary>
        void SetAdditionalData(string[] data);
    }
}
