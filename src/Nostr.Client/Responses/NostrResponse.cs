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
        public object[] AdditionalData { get; private set; } = Array.Empty<object>();

        /// <summary>
        /// Name of the source communicator/relay
        /// </summary>
        [JsonIgnore]
        public string CommunicatorName { get; internal set; } = string.Empty;

        /// <summary>
        /// Client timestamp of the received response, UTC
        /// </summary>
        [JsonIgnore]
        public DateTime ReceivedTimestamp { get; internal set; } = DateTime.UtcNow;

        /// <summary>
        /// Set additional data, should not be used outside of this library.
        /// Hidden behind explicit interface implementation to avoid accidental usage.
        /// </summary>
        void IHaveAdditionalData.SetAdditionalData(object[] data)
        {
            AdditionalData = data;
        }
    }
}
