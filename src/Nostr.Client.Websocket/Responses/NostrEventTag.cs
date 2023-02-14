using Newtonsoft.Json;
using Nostr.Client.Websocket.Json;
using System.Diagnostics;

namespace Nostr.Client.Websocket.Responses
{
    [DebuggerDisplay("Tag {TagIdentifier} additional: {AdditionalData.Length}")]
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrEventTag : IHaveAdditionalData
    {
        [ArrayProperty(0)]
        public string? TagIdentifier { get; init; }

        public object[] AdditionalData { get; set; } = Array.Empty<object>();
    }
}
