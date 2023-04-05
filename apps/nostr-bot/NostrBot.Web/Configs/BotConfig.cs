namespace NostrBot.Web.Configs
{
    public class BotConfig
    {
        public string? BotDescription { get; init; }

        public string? BotWhois { get; init; }

        public string[] BotAdminPubKeys { get; init; } = Array.Empty<string>();
        
        public bool ListenToGlobalFeed { get; init; }

        public string[] GlobalFeedKeywords { get; init; } = Array.Empty<string>();
    }
}
