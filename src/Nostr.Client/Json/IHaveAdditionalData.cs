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
        object[] AdditionalData { get; }

        /// <summary>
        /// Set additional data, should not be used outside of this library
        /// </summary>
        void SetAdditionalData(object[] data);
    }
}
