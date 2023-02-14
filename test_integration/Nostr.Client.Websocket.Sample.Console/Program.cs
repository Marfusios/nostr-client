using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nostr.Client.Websocket.Client;
using Nostr.Client.Websocket.Communicator;
using Nostr.Client.Websocket.Messages;
using Nostr.Client.Websocket.Requests;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

var exitEvent = new ManualResetEvent(false);

var logFactory = InitLogging();

AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
Console.CancelKeyPress += ConsoleOnCancelKeyPress;

Console.WriteLine("|======================|");
Console.WriteLine("|     NOSTR CLIENT     |");
Console.WriteLine("|======================|");
Console.WriteLine();

Log.Debug("====================================");
Log.Debug("              STARTING              ");
Log.Debug("====================================");

// var url = new Uri("wss://relay.snort.social");
var url = new Uri("wss://relay.damus.io");
// var url = new Uri("wss://eden.nostr.land");
using var communicator = new NostrWebsocketCommunicator(url, () =>
{
    var client = new ClientWebSocket();
    client.Options.SetRequestHeader("Origin", "http://localhost");
    return client;
});

communicator.Name = "Nostr-1";
communicator.ReconnectTimeout = null; //TimeSpan.FromSeconds(30);
communicator.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

communicator.ReconnectionHappened.Subscribe(info =>
    Log.Information($"Reconnection happened, type: {info.Type}"));
communicator.DisconnectionHappened.Subscribe(info =>
    Log.Information($"Disconnection happened, type: {info.Type}, reason: {info.CloseStatusDescription}"));

using var client = new NostrWebsocketClient(communicator, logFactory.CreateLogger<NostrWebsocketClient>());

client.Streams.EventStream.Subscribe(x =>
    Log.Information("{kind}: {content}", x.Event?.Kind, x.Event?.Content));
client.Streams.NoticeStream.Subscribe(x => Log.Information("Notice: {message}", x.Message));
client.Streams.EoseStream.Subscribe(x => Log.Information("EOSE of subscription {subscription}", x.Subscription));
client.Streams.UnknownMessageStream.Subscribe(x => Log.Information("Unknown {messageType} message, data: {data}", x.MessageType, JsonConvert.SerializeObject(x.AdditionalData)));
client.Streams.UnknownRawStream.Subscribe(x => Log.Warning("Unknown data: {data}", x.ToString()));

await communicator.StartOrFail();

// await Task.Delay(TimeSpan.FromSeconds(1));

Log.Debug("Sending request");

// client.Send(new object[] { "REQ", "timeline:global:all", new { kinds = new[] { 1, 6 }, since = 1676309050, until = 1676312650 } });

client.Send(new NostrRequest("timeline:pubkey:follows", new NostrFilter
{
    Authors = new[]
        {
            //"npub1v0lxxxxutpvrelsksy8cdhgfux9l6a42hsj2qzquu2zk7vc9qnkszrqj49",
            "63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed"
            //"3efdaebb1d8923ebd99c9e7ace3b4194ab45512e2be79c1b7d68d9243e0d2681",
            //"6b75a3b4832f265989254ca560b700da3343d707d2319e7a45f4e01afe4a0c31",
            //"82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2",
            //"63fe6318dc58583cfe16810f86dd09e18bfd76aabc24a0081ce2856f330504ed",
            //"e9e4276490374a0daf7759fd5f475deff6ffb9b0fc5fa98c902b5f4b2fe3bba2",
            //"604e96e099936a104883958b040b47672e0f048c98ac793f37ffe4c720279eb2",
            //"7b6461d02c6f0be1cacdcf968c4246105a2db51c7770993bf8bb25e59cedffa7",
            //"559dc217d58a74982396fea8b4e9af4b6fc9c96f11abb134da285fec028658fd",
            //"23518fb6a27dc83e475eca500600e2160c71e554786dfda9658d5d9f57819b66",
            //"91c9a5e1a9744114c6fe2d61ae4de82629eaaa0fb52f48288093c7e7e036f832",
            //"c4eabae1be3cf657bc1855ee05e69de9f059cb7a059227168b80b89761cbc4e0",
            //"04c915daefee38317fa734444acee390a8269fe5810b2241e5e6dd343dfbecc9",
            //"339d7804b6a69b7ef05a169d72ca3e977f64eb00ab6eedf21af0a2c2327691b3",
            //"85080d3bad70ccdcd7f74c29a44f55bb85cbcd3dd0cbb957da1d215bdb931204",
            //"020f2d21ae09bf35fcdfb65decf1478b846f5f728ab30c5eaabcd6d081a81c3e",
            //"6e468422dfb74a5738702a8823b9b28168abab8655faacb6853cd0ee15deee93",
            //"3bf0c63fcb93463407af97a5e5ee64fa883d107ef9e558472c4eb9aaaefa459d",
            //"e33fe65f1fde44c6dc17eeb38fdad0fceaf1cae8722084332ed1e32496291d42",
            //"a341f45ff9758f570a21b000c17d4e53a3a497c8397f26c0e6d61e5acffc7a98",
            //"83e818dfbeccea56b0f551576b3fd39a7a50e1d8159343500368fa085ccd964b",
            //"7fa56f5d6962ab1e3cd424e758c3002b8665f7b0d8dcee9fe9e288d7751ac194"
        },
    Kinds = new[]
    {
        NostrKind.Metadata,
        NostrKind.ShortTextNote,
        NostrKind.Reaction,
        NostrKind.Contacts,
        NostrKind.RecommendRelay,
        NostrKind.EventDeletion,
        NostrKind.Reporting,
        NostrKind.ClientAuthentication
    },
    Since = DateTime.UtcNow.AddHours(-12),
    Until = DateTime.UtcNow
}));

exitEvent.WaitOne();


Log.Debug("====================================");
Log.Debug("              STOPPING              ");
Log.Debug("====================================");
Log.CloseAndFlush();




static SerilogLoggerFactory InitLogging()
{
    Console.OutputEncoding = Encoding.UTF8;
    var executingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Directory.GetCurrentDirectory();
    var logPath = Path.Combine(executingDir, "logs", "verbose.log");
    var logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
        .WriteTo.Console(LogEventLevel.Debug,
            outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Literate)
        .CreateLogger();
    Log.Logger = logger;
    return new SerilogLoggerFactory(logger);
}

void CurrentDomainOnProcessExit(object? sender, EventArgs eventArgs)
{
    Log.Warning("Exiting process");
    exitEvent.Set();
}

void DefaultOnUnloading(AssemblyLoadContext assemblyLoadContext)
{
    Log.Warning("Unloading process");
    exitEvent.Set();
}

void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
{
    Log.Warning("Canceling process");
    e.Cancel = true;
    exitEvent.Set();
}