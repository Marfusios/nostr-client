![Logo](https://raw.githubusercontent.com/Marfusios/nostr-client/master/nostr.png)
# Nostr client 
[![.NET Core](https://github.com/Marfusios/nostr-client/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/Marfusios/nostr-client/actions/workflows/dotnet-core.yml) [![NuGet version](https://badge.fury.io/nu/Nostr.Client.svg)](https://badge.fury.io/nu/Nostr.Client) [![NuGet downloads](https://img.shields.io/nuget/dt/Nostr.Client)](https://www.nuget.org/packages/Nostr.Client)

This is a C# implementation of the Nostr protocol found here:

https://github.com/nostr-protocol/nips

Nostr protocol is based on websocket communication. 
This library keeps a reliable connection to get real-time data and fast execution of your commands. 

[Releases and breaking changes](https://github.com/Marfusios/nostr-client/releases)

### License: 
    Apache License 2.0

### Features

* installation via NuGet ([Nostr.Client](https://www.nuget.org/packages/Nostr.Client))
* targeting .NET 6.0 and higher (.NET Core, Linux/MacOS compatible)
* reactive extensions ([Rx.NET](https://github.com/Reactive-Extensions/Rx.NET))

### Usage

#### Receiving events

```csharp
var exitEvent = new ManualResetEvent(false);
var url = new Uri("wss://relay.damus.io");

using var communicator = new NostrWebsocketCommunicator(url);
using var client = new NostrWebsocketClient(communicator, null);

client.Streams.EventStream.Subscribe(response =>
{
    var ev = response.Event;
    Log.Information("{kind}: {content}", ev?.Kind, ev?.Content)
            
    if(ev is NostrMetadataEvent evm) {
        Log.Information("Name: {name}, about: {about}", evm.Metadata?.Name, evm.Metadata?.About);
    }
});

await communicator.Start();

exitEvent.WaitOne(TimeSpan.FromSeconds(30));
```

#### Sending event

```csharp
var ev = new NostrEvent
{
    Kind = NostrKind.ShortTextNote,
    CreatedAt = DateTime.UtcNow,
    Content = "Test message from C# client"
};

var key = NostrPrivateKey.FromBech32("nsec1xxx");
var signed = ev.Sign(key);

client.Send(new NostrEventRequest(signed));
```

#### Multi relays support

```csharp
var relays = new[]
{
    new NostrWebsocketCommunicator(new Uri("wss://relay.snort.social")),
    new NostrWebsocketCommunicator(new Uri("wss://relay.damus.io")),
    new NostrWebsocketCommunicator(new Uri("wss://nos.lol"))
};

var client = new NostrMultiWebsocketClient(NullLogger<NostrWebsocketClient>.Instance, relays);

client.Streams.EventStream.Subscribe(HandleEvent);

relays.ToList().ForEach(relay => relay.Start());
```

More usage examples:
* Tests ([link](tests/Nostr.Client.Tests))
* Console sample ([link](test_integration/Nostr.Client.Sample.Console/Program.cs))
* Blazor sample ([link](test_integration/Nostr.Client.Sample.Blazor), [deployed](https://nostrdebug.com))

![image](https://user-images.githubusercontent.com/3494837/219864079-b044327a-3db5-4f22-b738-3160a561b5f3.png)

### NIP's coverage

- [x] NIP-01: Basic protocol flow description
- [x] NIP-02: Contact List and Petnames (No petname support)
- [ ] NIP-03: OpenTimestamps Attestations for Events
- [ ] NIP-04: Encrypted Direct Message
- [ ] NIP-05: Mapping Nostr keys to DNS-based internet identifiers
- [ ] NIP-06: Basic key derivation from mnemonic seed phrase
- [ ] NIP-07: `window.nostr` capability for web browsers
- [ ] NIP-08: Handling Mentions
- [ ] NIP-09: Event Deletion
- [ ] NIP-10: Conventions for clients' use of `e` and `p` tags in text events
- [ ] NIP-11: Relay Information Document
- [ ] NIP-12: Generic Tag Queries
- [ ] NIP-13: Proof of Work
- [ ] NIP-14: Subject tag in text events
- [x] NIP-15: End of Stored Events Notice
- [x] NIP-19: bech32-encoded entities
- [x] NIP-20: Command Results
- [ ] NIP-21: `nostr:` Protocol handler (`web+nostr`)
- [ ] NIP-25: Reactions
- [ ] NIP-26: Delegated Event Signing (Display delegated signings only)
- [ ] NIP-28: Public Chat
- [ ] NIP-36: Sensitive Content
- [ ] NIP-40: Expiration Timestamp
- [ ] NIP-42: Authentication of clients to relays
- [ ] NIP-50: Search
- [ ] NIP-51: Lists
- [ ] NIP-65: Relay List Metadata

**Pull Requests are welcome!**

### Reconnecting

A built-in reconnection invokes after 1 minute (default) of not receiving any messages from the server. 
It is possible to configure that timeout via `communicator.ReconnectTimeout`. 
Also, a stream `ReconnectionHappened` sends information about a type of reconnection. 
However, if you are subscribed to low-rate channels, you will likely encounter that timeout - higher it to a few minutes or implement `ping-pong` interaction on your own every few seconds. 

In the case of Nostr relay outage, there is a built-in functionality that slows down reconnection requests 
(could be configured via `client.ErrorReconnectTimeout`, the default is 1 minute).

Beware that you **need to resubscribe to channels** after reconnection happens. You should subscribe to `ReconnectionHappened` stream and send subscription requests. 

### Testing

The library is prepared for replay testing. The dependency between `Client` and `Communicator` is via abstraction `INostrCommunicator`. There are two communicator implementations: 
* `NostrWebsocketCommunicator` - real-time communication with Nostr relay.
* `NostrFileCommunicator` - a simulated communication, raw data are loaded from files and streamed.

Feel free to implement `INostrCommunicator` on your own, for example, load raw data from database, cache, etc. 

Usage: 

```csharp
var communicator = new NostrFileCommunicator();
communicator.FileNames = new[]
{
    "data/nostr-data.txt"
};
communicator.Delimiter = "\n";

var client = new NostrWebsocketClient(communicator);
client.Streams.EventStream.Subscribe(trade =>
{
    // do something with an event
});

await communicator.Start();
```

### Multi-threading and other considerations

See [Websocket Client readme](https://github.com/Marfusios/websocket-client#multi-threading)
