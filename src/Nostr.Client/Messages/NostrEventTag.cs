using Newtonsoft.Json;
using Nostr.Client.Json;
using System.Diagnostics;

namespace Nostr.Client.Messages
{
    [DebuggerDisplay("Tag {TagIdentifier} additional: {AdditionalData.Length}")]
    [JsonConverter(typeof(ArrayConverter))]
    public class NostrEventTag : IHaveAdditionalStringData
    {
        public const string EventIdentifier = "e";
        public const string ProfileIdentifier = "p";
        public const string Identifier = "d";
        public const string CoordinatesIdentifier = "a";

        public NostrEventTag()
        {
        }

        public NostrEventTag(string? identifier, params string[] data)
        {
            TagIdentifier = identifier ?? string.Empty;
            AdditionalData = data;
        }

        [ArrayProperty(0)] 
        public string TagIdentifier { get; init; } = string.Empty;

        /// <summary>
        /// Additional unexpected data at higher indexes in the tags array
        /// </summary>
        public string[] AdditionalData { get; private set; } = Array.Empty<string>();

        /// <summary>
        /// Set additional data, should not be used outside of this library.
        /// Hidden behind explicit interface implementation to avoid accidental usage.
        /// </summary>
        void IHaveAdditionalStringData.SetAdditionalData(string[] data)
        {
            AdditionalData = data;
        }

        public NostrEventTag DeepClone()
        {
            return new NostrEventTag
            {
                TagIdentifier = TagIdentifier,
                AdditionalData = (string[])AdditionalData.Clone()
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
