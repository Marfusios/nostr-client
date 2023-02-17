using Newtonsoft.Json;
using Nostr.Client.Json;

namespace Nostr.Client.Responses
{
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrResponse : IHaveAdditionalData
    {
        [ArrayProperty(0)]
        public string? MessageType { get; init; }

        /// <summary>
        /// Additional data that are not yet handled by this library into static structure
        /// </summary>
        public object[] AdditionalData { get; set; } = Array.Empty<object>();
    }
}
