﻿namespace NostrBot.Web.Configs
{
    public class BotConfig
    {
        public string? BotDescription { get; init; }

        public string? BotWhois { get; init; }

        public string[] BotAdminPubKeys { get; init; } = Array.Empty<string>();

        public string[] BotIgnoreListPubKeys { get; init; } = Array.Empty<string>();

        public bool SlowdownReplies { get; init; }

        public double SlowdownPerTokenSec { get; init; } = 1;

        public int LimitForHistoricalTokens { get; init; } = 2000;

        public bool ListenToGlobalFeed { get; init; }

        public bool ReactToRootEventsInGlobalFeed { get; init; }

        public bool ReactToThreadsInGlobalFeed { get; init; }

        public bool ReactToThreadsInLiveChat { get; init; }

        public string[] GlobalFeedKeywords { get; init; } = Array.Empty<string>();
    }
}
