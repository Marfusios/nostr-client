![Logo](nostr.png)
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

More usage examples:
* Console sample ([link](test_integration/Nostr.Client.Sample.Console/Program.cs))
* Blazor sample ([link](test_integration/Nostr.Client.Sample.Blazor), [deployed](https://nostr.mkotas.cz))

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
- [ ] NIP-20: Command Results
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

There is a built-in reconnection which invokes after 1 minute (default) of not receiving any messages from the server. It is possible to configure that timeout via `communicator.ReconnectTimeoutMs`. Also, there is a stream `ReconnectionHappened` which sends information about a type of reconnection. However, if you are subscribed to low rate channels, it is very likely that you will encounter that timeout - higher the timeout to a few minutes or call `PingRequest` by your own every few seconds. 

In the case of Nostr relay outage, there is a built-in functionality which slows down reconnection requests (could be configured via `communicator.ErrorReconnectTimeoutMs`, the default is 1 minute).

Beware that you **need to resubscribe to channels** after reconnection happens. 

### Testing

The library is prepared for replay testing. The dependency between `Client` and `Communicator` is via abstraction `INostrCommunicator`. There are two communicator implementations: 
* `NostrWebsocketCommunicator` - a realtime communication with Nostr relay.
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

### Multi-threading

Observables from Reactive Extensions are single threaded by default. It means that your code inside subscriptions is called synchronously and as soon as the message comes from websocket API. 
It brings a great advantage of not to worry about synchronization, but if your code takes a longer time to execute it will block the receiving method, 
buffer the messages and may end up losing messages. For that reason consider to handle messages on the other thread and unblock receiving thread as soon as possible. 
I've prepared a few examples for you: 

#### Default behavior

Every subscription code is called on a main websocket thread. Every subscription is synchronized together. No parallel execution. It will block the receiving thread. 

```csharp
client
    .Streams
    .EventStream
    .Subscribe(event => { code1 });

client
    .Streams
    .NoticeStream
    .Subscribe(notice => { code2 });

// 'code1' and 'code2' are called in a correct order, according to websocket flow
// ----- code1 ----- code1 ----- ----- code1
// ----- ----- code2 ----- code2 code2 -----
```

#### Parallel subscriptions 

Every single subscription code is called on a separate thread. Every single subscription is synchronized, but different subscriptions are called in parallel. 

```csharp
client
    .Streams
    .EventStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Subscribe(event => { code1 });

client
    .Streams
    .NoticeStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Subscribe(notice => { code2 });

// 'code1' and 'code2' are called in parallel, do not follow websocket flow
// ----- code1 ----- code1 ----- code1 -----
// ----- code2 code2 ----- code2 code2 code2
```

 #### Parallel subscriptions with synchronization

In case you want to run your subscription code on the separate thread but still want to follow websocket flow through every subscription, use synchronization with gates: 

```csharp
private static readonly object GATE1 = new object();
client
    .Streams
    .EventStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Synchronize(GATE1)
    .Subscribe(event => { code1 });

client
    .Streams
    .NoticeStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Synchronize(GATE1)
    .Subscribe(notice => { code2 });

// 'code1' and 'code2' are called concurrently and follow websocket flow
// ----- code1 ----- code1 ----- ----- code1
// ----- ----- code2 ----- code2 code2 ----
```

### Async/Await integration

Using `async/await` in your subscribe methods is a bit tricky. Subscribe from Rx.NET doesn't `await` tasks, 
so it won't block stream execution and cause sometimes undesired concurrency. For example: 

```csharp
client
    .Streams
    .EventStream
    .Subscribe(async event => {
        // do smth 1
        await Task.Delay(5000); // waits 5 sec, could be HTTP call or something else
        // do smth 2
    });
```

That `await Task.Delay` won't block stream and subscribe method will be called multiple times concurrently. 
If you want to buffer messages and process them one-by-one, then use this: 

```csharp
client
    .Streams
    .EventStream
    .Select(event => Observable.FromAsync(async () => {
        // do smth 1
        await Task.Delay(5000); // waits 5 sec, could be HTTP call or something else
        // do smth 2
    }))
    .Concat() // executes sequentially
    .Subscribe();
```

If you want to process them concurrently (avoid synchronization), then use this

```csharp
client
    .Streams
    .EventStream
    .Select(event => Observable.FromAsync(async () => {
        // do smth 1
        await Task.Delay(5000); // waits 5 sec, could be HTTP call or something else
        // do smth 2
    }))
    .Merge() // executes concurrently
    // .Merge(4) you can limit concurrency with a parameter
    // .Merge(1) is same as .Concat()
    // .Merge(0) is invalid (throws exception)
    .Subscribe();
```

More info in [Github issue](https://github.com/dotnet/reactive/issues/459).

Don't worry about websocket connection, those sequential execution via `.Concat()` or `.Merge(1)` has no effect on receiving messages. 
It won't affect receiving thread, only buffers messages inside `EventStream`. 

But beware of [producer-consumer problem](https://en.wikipedia.org/wiki/Producer%E2%80%93consumer_problem) when the consumer will be too slow. Here is a [StackOverflow issue](https://stackoverflow.com/questions/11010602/with-rx-how-do-i-ignore-all-except-the-latest-value-when-my-subscribe-method-is) 
with an example how to ignore/discard buffered messages and always process only the last one. 
