using Newtonsoft.Json;
using System.Diagnostics;
using Websocket.Client;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("[{CommunicatorName}] {Message}")]
    public class NostrRawResponse
    {
        public ResponseMessage? Message { get; init; }

        /// <summary>
        /// Name of the source communicator/relay
        /// </summary>
        [JsonIgnore]
        public string CommunicatorName { get; internal set; } = string.Empty;
    }
}
