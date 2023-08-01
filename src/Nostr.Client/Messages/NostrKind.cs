namespace Nostr.Client.Messages
{
    public enum NostrKind
    {
        Metadata = 0,
        ShortTextNote = 1,
        RecommendRelay = 2,
        Contacts = 3,
        EncryptedDm = 4,
        EventDeletion = 5,
        Reserved = 6,
        Reaction = 7,
        BadgeAward = 8,

        GenericRepost = 16,

        // nip-28 public chat
        ChannelCreation = 40,
        ChannelMetadata = 41,
        ChannelMessage = 42,
        ChannelHideMessage = 43,
        ChanelMuteUser = 44,

        // nip-28 public chat reserved [45-49]

        FileMetadata = 1063,
        LiveChatMessage = 1311,

        Reporting = 1984,
        Label = 1985,

        ZapRequest = 9734,
        Zap = 9735,

        MuteList = 10000,
        PinList = 10001,
        RelayListMetadata = 10002,

        WalletInfo = 13194,
        ClientAuthentication = 22242,
        WalletRequest = 23194,
        WalletResponse = 23195,
        NostrConnect = 24133,
        HttpAuth = 27235,

        CategorizedPeopleList = 30000,
        CategorizedBookmarkList = 30001,

        ProfileBadges = 30008,
        BadgeDefinition = 30009,

        LongFormContent = 30023,
        DraftLongFormContent = 30024,

        ApplicationSpecificData = 30078,

        LiveEvent = 30311,

        // nip-16 regular events                   [ 1000- 9999]
        // nip-16 replaceable events               [10000-19999]
        // nip-16 ephemeral  events                [20000-29999]
        // nip-33 parameterized replaceable events [30000-39999]
    }
}
