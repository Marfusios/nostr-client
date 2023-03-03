namespace Nostr.Client.Client
{
    public interface INostrClient : IDisposable
    {
        /// <summary>
        /// Provided message streams
        /// </summary>
        NostrClientStreams Streams { get; }

        /// <summary>
        /// Serializes request and sends message via websocket communicator. 
        /// It logs and re-throws every exception. 
        /// </summary>
        /// <param name="request">Request/message to be sent</param>
        void Send<T>(T request);
    }
}
