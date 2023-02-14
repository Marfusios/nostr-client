![Logo](bitfinex_logo.png)
# Bitfinex & Ethfinex websocket API client 
[![Build Status](https://travis-ci.org/Marfusios/bitfinex-client-websocket.svg?branch=master)](https://travis-ci.org/Marfusios/bitfinex-client-websocket) [![NuGet version](https://badge.fury.io/nu/Bitfinex.Client.Websocket.svg)](https://badge.fury.io/nu/Bitfinex.Client.Websocket) [![NuGet downloads](https://img.shields.io/nuget/dt/Bitfinex.Client.Websocket)](https://www.nuget.org/packages/Bitfinex.Client.Websocket)

This is a C# implementation of the Bitfinex & Ethfinex websocket API version 2.0 found here:

https://bitfinex.readme.io/v2/docs ([Ethfinex](https://www.ethfinex.com/api_docs))

You can **do** almost **everything with** provided **websocket API**. Using REST API is unnecessary! 
As a benefit, you will get real-time data and fast execution of your commands. 

[Releases and breaking changes](https://github.com/Marfusios/bitfinex-client-websocket/releases)

### License: 
    Apache License 2.0

### Features

* installation via NuGet ([Bitfinex.Client.Websocket](https://www.nuget.org/packages/Bitfinex.Client.Websocket))
* public and authenticated API
* targeting .NET Standard 2.0 (.NET Core, Linux/MacOS compatible)
* reactive extensions ([Rx.NET](https://github.com/Reactive-Extensions/Rx.NET))
* integrated logging abstraction ([LibLog](https://github.com/damianh/LibLog))

### Usage

```csharp
var exitEvent = new ManualResetEvent(false);
var url = BitfinexValues.ApiWebsocketUrl;

using (var communicator = new BitfinexWebsocketCommunicator(url))
{
    using (var client = new BitfinexWebsocketClient(communicator))
    {
        client.Streams.InfoStream.Subscribe(info =>
        {
            Log.Information($"Info received, reconnection happened, resubscribing to streams");
            
            await client.Send(new PingRequest() {Cid = 123456});
            //await client.Send(new TickerSubscribeRequest("BTC/USD"));
        });

        client.Streams.PongStream.Subscribe(pong =>
        {
            Console.WriteLine($"Pong received! Id: {pong.Cid}") // Pong received! Id: 123456
            exitEvent.Set();
        });

        await communicator.Start();

        exitEvent.WaitOne(TimeSpan.FromSeconds(30));
    }
}
```

More usage examples:
* integration tests ([link](test_integration/Bitfinex.Client.Websocket.Tests.Integration))
* console sample ([link](test_integration/Bitfinex.Client.Websocket.Sample/Program.cs))
* desktop sample ([link](test_integration/Bitfinex.Client.Websocket.Sample.WinForms))

### API coverage

| PUBLIC                 |    Covered     |
|------------------------|:--------------:|
| Info                   |  ✔            |
| Ping-Pong              |  ✔            |
| Errors                 |  ✔            |
| Configuration          |  ✔            |
| Channel subscribing    |  ✔            |
| Channel unsubscribing  |  ✔            |
| Ticker                 |  ✔            |
| Ticker - funding       |                |
| Trades                 |  ✔            |
| Trades - funding       |  ✔            |
| Books                  |  ✔            |
| Books - funding        |  ✔            |
| Raw books              |  ✔            |
| Raw books - funding    |  ✔            |
| Candles                |  ✔            |
| Funding                |  ✔            |
| Sequencing             |  ✔            |
| Server timestamp       |  ✔            |
| Book checksum          |  ✔            |

| AUTHENTICATED          |    Covered     |
|------------------------|:--------------:|
| Account info           |  ✔            |
| Orders                 |  ✔            |
| Positions              |  ✔            |
| Trades                 |  ✔            |
| Funding                |                |
| Wallets                |  ✔            |
| Balance                |  ✔            |
| Notifications          |  ✔            |

| AUTHENTICATED - INPUT  |    Covered     |
|------------------------|:--------------:|
| New order              |  ✔            |
| Update order           |  ✔            |
| Cancel order           |  ✔            |
| Cancel order multi     |  ✔            |
| Order multi-op         |                |
| New offer              |                |
| Cancel offer           |                |
| Calc                   |  ✔            |

**Pull Requests are welcome!**

### Other websocket libraries

<table>
<tr>

<td>
<a href="https://github.com/Marfusios/crypto-websocket-extensions"><img src="https://raw.githubusercontent.com/Marfusios/crypto-websocket-extensions/master/cwe_logo.png" height="80px"></a>
<br />
<a href="https://github.com/Marfusios/crypto-websocket-extensions">Extensions</a>
<br />
<span>All order books together, etc.</span>
</td>

<td>
<a href="https://github.com/Marfusios/bitmex-client-websocket"><img src="https://user-images.githubusercontent.com/1294454/27766319-f653c6e6-5ed4-11e7-933d-f0bc3699ae8f.jpg"></a>
<br />
<a href="https://github.com/Marfusios/bitmex-client-websocket">Bitmex</a>
</td>

<td>
<a href="https://github.com/Marfusios/binance-client-websocket"><img src="https://user-images.githubusercontent.com/1294454/29604020-d5483cdc-87ee-11e7-94c7-d1a8d9169293.jpg"></a>
<br />
<a href="https://github.com/Marfusios/binance-client-websocket">Binance</a>
</td>

<td>
<a href="https://github.com/Marfusios/coinbase-client-websocket"><img src="https://user-images.githubusercontent.com/1294454/41764625-63b7ffde-760a-11e8-996d-a6328fa9347a.jpg"></a>
<br />
<a href="https://github.com/Marfusios/coinbase-client-websocket">Coinbase</a>
</td>

</tr>
</table>

### Placing orders

Bitfinex supports input authenticated API via websockets. 
You are able to place, update, cancel orders. Also via multi batch. 
Usage: 

```csharp
// placing buy
client.Send(new NewOrderRequest(gid: 33, cid: 100, "ETH/USD", OrderType.Limit, 0.2, 163) {Flags = OrderFlag.PostOnly});

// palcing sell
client.Send(new NewOrderRequest(gid: 33, cid: 200, "ETH/USD", OrderType.Limit, -0.2, 188) { Flags = OrderFlag.PostOnly });

// updating by client id
client.Send(new UpdateOrderRequest(new CidPair(100, DateTime.UtcNow)) { Amount = 0.3, Price = 161});

// canceling by client id
client.Send(new CancelOrderRequest(new CidPair(100, DateTime.UtcNow)));

// other canceling options
client.Send(CancelMultiOrderRequest.CancelEverything());
client.Send(CancelMultiOrderRequest.CancelGroup(33));
client.Send(new CancelMultiOrderRequest(new[]
{
    new CidPair(100, DateTime.UtcNow),
    new CidPair(200, DateTime.UtcNow)
}));
```

### Reconnecting

There is a built-in reconnection which invokes after 1 minute (default) of not receiving any messages from the server. It is possible to configure that timeout via `communicator.ReconnectTimeoutMs`. Also, there is a stream `ReconnectionHappened` which sends information about a type of reconnection. However, if you are subscribed to low rate channels, it is very likely that you will encounter that timeout - higher the timeout to a few minutes or call `PingRequest` by your own every few seconds. 

In the case of Bitfinex outage, there is a built-in functionality which slows down reconnection requests (could be configured via `communicator.ErrorReconnectTimeoutMs`, the default is 1 minute).

Beware that you **need to resubscribe to channels** after reconnection happens. You should subscribe to `Streams.InfoStream`, `Streams.AuthenticationStream` and send subscriptions requests (see [#12](https://github.com/Marfusios/bitfinex-client-websocket/issues/12) for example). 

### Backtesting

The library is prepared for backtesting. The dependency between `Client` and `Communicator` is via abstraction `IBitfinexCommunicator`. There are two communicator implementations: 
* `BitfinexWebsocketCommunicator` - a realtime communication with Bitfinex via websocket API.
* `BitfinexFileCommunicator` - a simulated communication, raw data are loaded from files and streamed. If you are **interested in buying historical raw data** (trades, order book events), contact me.

Feel free to implement `IBitfinexCommunicator` on your own, for example, load raw data from database, cache, etc. 

Usage: 

```csharp
var communicator = new BitfinexFileCommunicator();
communicator.FileNames = new[]
{
    "data/bitfinex_raw_2018-11-12.txt"
};
communicator.Delimiter = ";;";

var client = new BitfinexWebsocketClient(communicator);
client.Streams.TradesStream.Subscribe(trade =>
{
    // do something with trade
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
    .TradesStream
    .Subscribe(trade => { code1 });

client
    .Streams
    .BookStream
    .Subscribe(book => { code2 });

// 'code1' and 'code2' are called in a correct order, according to websocket flow
// ----- code1 ----- code1 ----- ----- code1
// ----- ----- code2 ----- code2 code2 -----
```

#### Parallel subscriptions 

Every single subscription code is called on a separate thread. Every single subscription is synchronized, but different subscriptions are called in parallel. 

```csharp
client
    .Streams
    .TradesStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Subscribe(trade => { code1 });

client
    .Streams
    .BookStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Subscribe(book => { code2 });

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
    .TradesStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Synchronize(GATE1)
    .Subscribe(trade => { code1 });

client
    .Streams
    .BookStream
    .ObserveOn(TaskPoolScheduler.Default)
    .Synchronize(GATE1)
    .Subscribe(book => { code2 });

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
    .TradesStream
    .Subscribe(async trade => {
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
    .TradesStream
    .Select(trade => Observable.FromAsync(async () => {
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
    .TradesStream
    .Select(trade => Observable.FromAsync(async () => {
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

More info on [Github issue](https://github.com/dotnet/reactive/issues/459).

Don't worry about websocket connection, those sequential execution via `.Concat()` or `.Merge(1)` has no effect on receiving messages. 
It won't affect receiving thread, only buffers messages inside `TradesStream`. 

But beware of [producer-consumer problem](https://en.wikipedia.org/wiki/Producer%E2%80%93consumer_problem) when the consumer will be too slow. Here is a [StackOverflow issue](https://stackoverflow.com/questions/11010602/with-rx-how-do-i-ignore-all-except-the-latest-value-when-my-subscribe-method-is) 
with an example how to ignore/discard buffered messages and always process only the last one. 


### Desktop application (WinForms or WPF)

Due to the large amount of questions about integration of this library into a desktop application (old full .NET Framework), I've prepared WinForms example ([link](test_integration/Bitfinex.Client.Websocket.Sample.WinForms)). 

![WinForms example screen](test_integration/Bitfinex.Client.Websocket.Sample.WinForms/winforms_example_app.png)

### Available for help
I do consulting, please don't hesitate to contact me if you have a custom solution you would like me to implement ([web](http://mkotas.cz/), 
<m@mkotas.cz>)

Donations gratefully accepted.
* [![Donate with Bitcoin](https://en.cryptobadges.io/badge/small/1HfxKZhvm68qK3gE8bJAdDBWkcZ2AFs9pw)](https://en.cryptobadges.io/donate/1HfxKZhvm68qK3gE8bJAdDBWkcZ2AFs9pw)
* [![Donate with Litecoin](https://en.cryptobadges.io/badge/small/LftdENE8DTbLpV6RZLKLdzYzVU82E6dz4W)](https://en.cryptobadges.io/donate/LftdENE8DTbLpV6RZLKLdzYzVU82E6dz4W)
* [![Donate with Ethereum](https://en.cryptobadges.io/badge/small/0xb9637c56b307f24372cdcebd208c0679d4e48a47)](https://en.cryptobadges.io/donate/0xb9637c56b307f24372cdcebd208c0679d4e48a47)
