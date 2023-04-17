using BTCPayServer.Lightning;
using Nostr.Client.Json;
using Nostr.Client.Messages;
using Nostr.Client.Messages.Zaps;

namespace Nostr.Client.Tests
{
    public class NostrZapTests
    {

        [Fact]
        public void ZapReceipt_ShouldDeserializeCorrectly()
        {
            var serialized = "{\r\n  " +
                     "\"id\": \"75839529323e6dc3a551fd92f4665fe81d192270c67c482c76238522965211a2\",\r\n  " +
                     "\"pubkey\": \"be1d89794bf92de5dd64c1e60f6a2c70c140abac9932418fee30c5c637fe9479\",\r\n  " +
                     "\"created_at\": 1681748476,\r\n  " +
                     "\"kind\": 9735,\r\n  " +
                     "\"tags\": [\r\n    " +
                     "[\r\n      " +
                     "\"p\",\r\n      \"875685e12bdeaaa7a207d8d25c3fd432a8af307b80f8a5226777b50b0aa2f846\"\r\n    ],\r\n    " +
                     "[\r\n      " +
                     "\"e\",\r\n      \"49113ee36916684cad14ab94b0e579455e58adedfb5b8952d1b42b56384b438e\"\r\n    ],\r\n    " +
                     "[\r\n      " +
                     "\"bolt11\",\r\n      \"lnbc250n1pjr6u0wpp503cpw05zqcuxfkqvzep6wrjdpqfc076yw67kp8hh84f00vrvmq0shp5glhp6r5ds93skagf8feuq6u47gdur96ma43zv4k2usx5r5rjkfpscqzpgxqzfvsp5uxwycpuny2vyjx9kl3u8a6km0vjeghqg72t53mxgps5k4wuzcusq9qyyssql7rtas2wgwxehl4ds4fjx89cls0fwf5dz9759znyawrghjn50gfkqqra78wr7778am4dk7q5u069mx48hpfna3g76tj3tsel8te6nwgptrw9hr\"\r\n    ],\r\n    " +
                     "[\r\n      " +
                     "\"description\",\r\n      \"{\\\"pubkey\\\":\\\"5ce459cafd0d464375b872cb48826012bd1c017566c536d56440b5462591be2f\\\",\\\"content\\\":\\\"\\\",\\\"id\\\":\\\"5f68bf9a505234d6ce0fc3368f4a7dfe6b624165b7b77e9814a02643a729822b\\\",\\\"created_at\\\":1681748461,\\\"sig\\\":\\\"5754f5cba1c347a5b34924b5ca1df9562514851e9f2fe7118a46f1242dc0ede7f9fd3ec3fb52773dc13aa65d3c560214ca8be389fbd01ac40954d43e6a06d632\\\",\\\"kind\\\":9734,\\\"tags\\\":[[\\\"e\\\",\\\"49113ee36916684cad14ab94b0e579455e58adedfb5b8952d1b42b56384b438e\\\"],[\\\"p\\\",\\\"875685e12bdeaaa7a207d8d25c3fd432a8af307b80f8a5226777b50b0aa2f846\\\"],[\\\"relays\\\",\\\"wss://relay.nostriches.org/\\\",\\\"wss://nostr.wine/\\\",\\\"wss://nostr.kollider.xyz/\\\",\\\"wss://eden.nostr.land\\\",\\\"wss://no.str.cr/\\\",\\\"wss://nostr.fmt.wiz.biz/\\\",\\\"wss://nos.lol/\\\",\\\"wss://nos.lol\\\",\\\"wss://nostr.wine\\\",\\\"wss://e.nos.lol/\\\"]]}\"\r\n    ]\r\n  " +
                     "],\r\n  " +
                     "\"content\": \"\",\r\n  " +
                     "\"sig\": \"faa5c804c67d2d3b00d415453a669e99a383e30bbf8f9d3ac4c1572026770124d4443544a86911e076de142df69b7a7718dbb79eee6b2bce773be6138cbac482\"\r\n" +
                     "}";

            var deserialized = NostrJson.Deserialize<NostrZapReceiptEvent>(serialized);

            Assert.NotNull(deserialized);
            Assert.Equal(NostrKind.Zap, deserialized.Kind);
            Assert.Equal("875685e12bdeaaa7a207d8d25c3fd432a8af307b80f8a5226777b50b0aa2f846", deserialized.RecipientPubkey);

            var invoice = deserialized.DecodeBolt11();

            Assert.NotNull(invoice);
            Assert.Equal(25, invoice.MinimumAmount.ToUnit(LightMoneyUnit.Satoshi));
        }
    }
}
