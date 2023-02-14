using System.Reactive.Linq;
using System.Reactive.Subjects;
using Nostr.Client.Websocket.Responses;
using Websocket.Client;

namespace Nostr.Client.Websocket.Client
{
    public class NostrClientStreams
    {
        internal readonly Subject<NostrEventResponse> EventSubject = new();
        internal readonly Subject<NostrNoticeResponse> NoticeSubject = new();
        internal readonly Subject<NostrEoseResponse> EoseSubject = new();

        internal readonly Subject<NostrResponse> UnknownMessageSubject = new();
        internal readonly Subject<ResponseMessage> UnknownRawSubject = new();

        /// <summary>
        /// Requested Nostr events
        /// </summary>
        public IObservable<NostrEventResponse> EventStream => EventSubject.AsObservable();

        /// <summary>
        /// Human-readable messages
        /// </summary>
        public IObservable<NostrNoticeResponse> NoticeStream => NoticeSubject.AsObservable();

        /// <summary>
        /// Information that all stored events have been sent out
        /// </summary>
        public IObservable<NostrEoseResponse> EoseStream => EoseSubject.AsObservable();

        /// <summary>
        /// Unknown and unhandled messages
        /// </summary>
        public IObservable<NostrResponse> UnknownMessageStream => UnknownMessageSubject.AsObservable();

        /// <summary>
        /// Unknown messages that are not even in the parseable format
        /// </summary>
        public IObservable<ResponseMessage> UnknownRawStream => UnknownRawSubject.AsObservable();

    }
}
