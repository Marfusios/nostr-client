using Nostr.Client.Websocket.Utils;

namespace Nostr.Client.Websocket.Tests;

public class NostrConverterTests
{
    [Theory]
    [InlineData("npub1dd668dyr9un9nzf9fjjkpdcqmge584c86gceu7j97nsp4lj2pscs0xk075", "npub", "6b75a3b4832f265989254ca560b700da3343d707d2319e7a45f4e01afe4a0c31")]
    [InlineData("npub1v0lxxxxutpvrelsksy8cdhgfux9l6a42hsj2qzquu2zk7vc9qnkszrqj49", "npub", "63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed")]
    [InlineData("nsec169ajg0pst4246c50pu353lluc2klwyfsw8e69xegmyama23rlf8shjvswd", "nsec", "d17b243c305d555d628f0f2348fffcc2adf7113071f3a29b28d93bbeaa23fa4f")]
    public void ToHex_ShouldConvertCorrectly(string bech32, string expectedHrp, string expectedHex)
    {
        var converted = NostrConverter.ToHex(bech32, out var hrp);
        Assert.Equal(expectedHex, converted);
        Assert.Equal(expectedHrp, hrp);
    }

    [Theory]
    [InlineData("6b75a3b4832f265989254ca560b700da3343d707d2319e7a45f4e01afe4a0c31", "npub1dd668dyr9un9nzf9fjjkpdcqmge584c86gceu7j97nsp4lj2pscs0xk075")]
    [InlineData("63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed", "npub1v0lxxxxutpvrelsksy8cdhgfux9l6a42hsj2qzquu2zk7vc9qnkszrqj49")]
    public void ToNpub_ShouldConvertCorrectly(string hex, string expected)
    {
        var converted = NostrConverter.ToNpub(hex);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("d17b243c305d555d628f0f2348fffcc2adf7113071f3a29b28d93bbeaa23fa4f", "nsec169ajg0pst4246c50pu353lluc2klwyfsw8e69xegmyama23rlf8shjvswd")]
    public void ToNsec_ShouldConvertCorrectly(string hex, string expected)
    {
        var converted = NostrConverter.ToNsec(hex);
        Assert.Equal(expected, converted);
    }
}