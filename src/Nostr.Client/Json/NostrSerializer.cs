using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Nostr.Client.Json
{
    public class NostrSerializer
    {
        /// <summary>
        /// Unified JSON settings
        /// </summary>
        public static JsonSerializerSettings Settings => new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new UnixDateTimeConverter()
            },
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Unified JSON settings that serializes messages into array
        /// </summary>
        public static JsonSerializerSettings ArraySettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new ArrayConverter(),
                new UnixDateTimeConverter()
            },
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Custom preconfigured serializer
        /// </summary>
        public static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);

        /// <summary>
        /// Deserialize with Nostr JSON settings, swallow any exception
        /// </summary>
        public static T? DeserializeSafely<T>(string? content)
        {
            try
            {
                return content == null ?
                    default :
                    JsonConvert.DeserializeObject<T>(content, Settings);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
