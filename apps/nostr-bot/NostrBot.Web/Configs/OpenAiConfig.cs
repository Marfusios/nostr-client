namespace NostrBot.Web.Configs
{
    public class OpenAiConfig
    {
        public string ApiKey { get; init; } = null!;
        public string Organization { get; init; } = null!;

        public string Model { get; init; } = "gpt-3.5-turbo";
        public bool ModelSupportsVision { get; init; }
        public string ImageModel { get; init; } = "dall-e-3";
        public int ImageCount { get; init; } = 1;
        public string ImageQuality { get; init; } = "hd";

        public double? Temperature { get; init; }

        public int? MaxTokens { get; init; }

        public double? PresencePenalty { get; init; }

        public double? FrequencyPenalty { get; init; }
    }
}
