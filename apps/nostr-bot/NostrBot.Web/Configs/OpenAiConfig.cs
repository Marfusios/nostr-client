namespace NostrBot.Web.Configs
{
    public class OpenAiConfig
    {
        public string ApiKey { get; init; } = null!;
        public string Organization { get; init; } = null!;

        public string Model { get; init; } = "gpt-3.5-turbo";

        public double? Temperature { get; init; }

        public int? MaxTokens { get; init; }

        public double? PresencePenalty { get; init; }

        public double? FrequencyPenalty { get; init; }
    }
}
