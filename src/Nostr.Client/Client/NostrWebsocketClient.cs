using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nostr.Client.Communicator;
using Nostr.Client.Json;
using Nostr.Client.Messages;
using Nostr.Client.Responses;
using Websocket.Client;

namespace Nostr.Client.Client
{
    /// <summary>
    /// Nostr client that connects to a single relay. 
    /// Subscribe to `Streams` to handle received messages.
    /// </summary>
    public class NostrWebsocketClient : INostrClient
    {
        private readonly ILogger<NostrWebsocketClient> _logger;
        private readonly IDisposable? _messageReceivedSubscription;
        private readonly JsonSerializerSettings _jsonSettings;

        public NostrWebsocketClient(INostrCommunicator communicator, ILogger<NostrWebsocketClient>? logger)
        {
            _logger = logger ?? new NullLogger<NostrWebsocketClient>();
            Communicator = communicator;
            _messageReceivedSubscription = Communicator.MessageReceived.Subscribe(HandleMessage);

            // cache settings, avoid getting new instance all the time
            _jsonSettings = NostrSerializer.Settings;
        }

        public void Dispose()
        {
            _messageReceivedSubscription?.Dispose();
        }

        /// <summary>
        /// Underlying communicator
        /// </summary>
        public INostrCommunicator Communicator { get; }

        /// <summary>
        /// Provided message streams
        /// </summary>
        public NostrClientStreams Streams { get; } = new();

        /// <summary>
        /// Serializes request and sends message via websocket communicator. 
        /// It logs and re-throws every exception. 
        /// </summary>
        /// <param name="request">Request/message to be sent</param>
        public void Send<T>(T request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var serialized = JsonConvert.SerializeObject(request, _jsonSettings);
                Communicator.Send(serialized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while sending message '{request}'. Error: {error}", request, e.Message);
                throw;
            }
        }

        private void HandleMessage(ResponseMessage message)
        {
            try
            {
                var formatted = (message.Text ?? string.Empty).Trim();
                if (formatted.StartsWith("["))
                {
                    OnArrayMessage(formatted, message);
                    return;
                }

                Streams.UnknownRawSubject.OnNext(Raw(message));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while receiving message");
                Streams.UnknownRawSubject.OnNext(Raw(message));
            }
        }

        private void OnArrayMessage(string formatted, ResponseMessage originalMessage)
        {
            var parsed = DeserializeRaw(formatted);
            if (parsed.Count <= 0)
            {
                Streams.UnknownRawSubject.OnNext(Raw(originalMessage));
                return;
            }

            var messageTypeToken = parsed[0];
            if (messageTypeToken.Type != JTokenType.String)
            {
                Streams.UnknownRawSubject.OnNext(Raw(originalMessage));
                return;
            }

            var messageType = messageTypeToken.ToString()?.ToUpperInvariant();
            switch (messageType)
            {
                case NostrMessageTypes.Event:
                    Streams.EventSubject.OnNext(Deserialize<NostrEventResponse>(formatted));
                    return;
                case NostrMessageTypes.Eose:
                    Streams.EoseSubject.OnNext(Deserialize<NostrEoseResponse>(formatted));
                    return;
                case NostrMessageTypes.Notice:
                    Streams.NoticeSubject.OnNext(Deserialize<NostrNoticeResponse>(formatted));
                    return;
                case NostrMessageTypes.Ok:
                    Streams.OkSubject.OnNext(Deserialize<NostrOkResponse>(formatted));
                    return;
                default:
                    Streams.UnknownMessageSubject.OnNext(Deserialize<NostrResponse>(formatted));
                    return;
            }
        }

        private JArray DeserializeRaw(string content)
        {
            return JsonConvert.DeserializeObject<JArray>(content, _jsonSettings) ??
                               throw new InvalidOperationException("Deserialized initial array is null, cannot continue");
        }

        private T Deserialize<T>(string content) where T : NostrResponse
        {
            var deserialized = JsonConvert.DeserializeObject<T>(content, _jsonSettings) ??
                   throw new InvalidOperationException("Deserialized message is null, cannot continue");
            deserialized.CommunicatorName = Communicator.Name;
            return deserialized;
        }

        private NostrRawResponse Raw(ResponseMessage message)
        {
            return new NostrRawResponse
            {
                Message = message,
                CommunicatorName = Communicator.Name
            };
        }
    }
}
