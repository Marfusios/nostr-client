namespace Nostr.Client.Json
{
    public interface IHaveAdditionalData
    {
        public object[] AdditionalData { get; internal set; }
    }
}
