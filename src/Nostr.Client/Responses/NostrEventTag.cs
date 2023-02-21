using Newtonsoft.Json;
using Nostr.Client.Json;
using System.Diagnostics;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("Tag {TagIdentifier} additional: {AdditionalData.Length}")]
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrEventTag : IHaveAdditionalData
    {
        [ArrayProperty(0)]
        public string? TagIdentifier { get; init; }

        /// <summary>
        /// Additional unexpected data at higher indexes in the tags array
        /// </summary>
        public object[] AdditionalData { get; set; } = Array.Empty<object>();
    }
}
