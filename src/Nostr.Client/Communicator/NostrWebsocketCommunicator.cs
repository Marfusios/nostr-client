using System.Net.WebSockets;
using Websocket.Client;

namespace Nostr.Client.Communicator
{
    public class NostrWebsocketCommunicator : WebsocketClient, INostrCommunicator
    {
        /// <inheritdoc />
        public NostrWebsocketCommunicator(Uri url, Func<ClientWebSocket>? clientFactory = null)
            : base(url, clientFactory)
        {
        }
    }
}
