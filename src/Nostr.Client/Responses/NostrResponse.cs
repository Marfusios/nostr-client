using Newtonsoft.Json;
using Nostr.Client.Json;
using System.Diagnostics;

namespace Nostr.Client.Responses
{
    [JsonConverter(typeof(ArrayConverter))]
    [DebuggerDisplay("[{CommunicatorName}] {MessageType}")]
    public class NostrResponse : IHaveAdditionalData
    {
        [ArrayProperty(0)]
        public string? MessageType { get; init; }

        /// <summary>
        /// Additional data that are not yet handled by this library (parsed into properties)
        /// </summary>
        public object[] AdditionalData { get; set; } = Array.Empty<object>();

        /// <summary>
        /// Name of the source communicator/relay
        /// </summary>
        [JsonIgnore]
        public string CommunicatorName { get; internal set; } = string.Empty;
    }
}
