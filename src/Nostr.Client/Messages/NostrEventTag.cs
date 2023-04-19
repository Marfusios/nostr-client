using Newtonsoft.Json;
using Nostr.Client.Json;
using System.Diagnostics;

namespace Nostr.Client.Messages
{
    [DebuggerDisplay("Tag {TagIdentifier} additional: {AdditionalData.Length}")]
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrEventTag : IHaveAdditionalData
    {
        public const string EventIdentifier = "e";
        public const string ProfileIdentifier = "p";

        public NostrEventTag()
        {
        }

        public NostrEventTag(string? identifier, params object[] data)
        {
            TagIdentifier = identifier;
            AdditionalData = data;
        }

        [ArrayProperty(0)]
        public string? TagIdentifier { get; init; }

        /// <summary>
        /// Additional unexpected data at higher indexes in the tags array
        /// </summary>
        public object[] AdditionalData { get; private set; } = Array.Empty<object>();

        /// <summary>
        /// Set additional data, should not be used outside of this library.
        /// Hidden behind explicit interface implementation to avoid accidental usage.
        /// </summary>
        void IHaveAdditionalData.SetAdditionalData(object[] data)
        {
            AdditionalData = data;
        }

        public NostrEventTag DeepClone()
        {
            return new NostrEventTag
            {
                TagIdentifier = TagIdentifier,
                AdditionalData = (object[])AdditionalData.Clone()
            };
        }

        public static NostrEventTag Event(string eventId)
        {
            return new NostrEventTag(EventIdentifier, eventId);
        }

        public static NostrEventTag Profile(string pubkey)
        {
            return new NostrEventTag(ProfileIdentifier, pubkey);
        }
    }
}
