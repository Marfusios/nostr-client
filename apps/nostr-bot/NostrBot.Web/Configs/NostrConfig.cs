namespace NostrBot.Web.Configs
{
    public class NostrConfig
    {
        public string PrivateKey { get; init; } = null!;
        public string[] Relays { get; init; } = null!;
    }
}
