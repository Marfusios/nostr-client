namespace Nostr.Client.Websocket.Json
{
    public interface IHaveAdditionalData
    {
        public object[] AdditionalData { get; internal set; }
    }
}
