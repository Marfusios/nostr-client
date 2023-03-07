using System.Diagnostics;
using Nostr.Client.Json;

namespace Nostr.Client.Responses
{
    [DebuggerDisplay("[{CommunicatorName}] {MessageType} - success: {Accepted} {Message}")]
    public class NostrOkResponse : NostrResponse
    {
        /// <summary>
        /// Related event id
        /// </summary>
        [ArrayProperty(1)]
        public string? EventId { get; init; }

        /// <summary>
        /// Returns true when the event was accepted and stored on the server (even for duplicates). 
        /// Returns false when the event was rejected and not stored.
        /// </summary>
        [ArrayProperty(2)]
        public bool Accepted { get; init; }

        /// <summary>
        /// Additional information as to why the command succeeded or failed.
        /// </summary>
        [ArrayProperty(3)]
        public string? Message { get; init; }
    }
}
