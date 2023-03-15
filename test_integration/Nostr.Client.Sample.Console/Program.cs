using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Nostr.Client.Client;
using Nostr.Client.Communicator;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Sample.Console;
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

var relays = new[]
{
    new Uri("wss://relay.snort.social"),
    new Uri("wss://relay.damus.io"),
    new Uri("wss://eden.nostr.land"),
    new Uri("wss://nostr-pub.wellorder.net"),
    new Uri("wss://nos.lol"),
};

using var multiClient = new NostrMultiWebsocketClient(logFactory.CreateLogger<NostrWebsocketClient>());
var communicators = new List<NostrWebsocketCommunicator>();

foreach (var relay in relays)
{
    var communicator = CreateCommunicator(relay);
    communicators.Add(communicator);
    multiClient.RegisterCommunicator(communicator);
}

var viewer = new NostrViewer(multiClient);

viewer.Subscribe();

communicators.ForEach(x => x.Start());

viewer.SendRequests();

exitEvent.WaitOne();

Log.Debug("====================================");
Log.Debug("              STOPPING              ");
Log.Debug("====================================");
Log.CloseAndFlush();

foreach (var communicator in communicators)
{
    await communicator.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
    await Task.Delay(500);
    communicator.Dispose();
}

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
            theme: AnsiConsoleTheme.Code)
        .CreateLogger();
    Log.Logger = logger;
    return new SerilogLoggerFactory(logger);
}

NostrWebsocketCommunicator CreateCommunicator(Uri uri)
{
    var comm = new NostrWebsocketCommunicator(uri, () =>
    {
        var client = new ClientWebSocket();
        client.Options.SetRequestHeader("Origin", "http://localhost");
        return client;
    });

    comm.Name = uri.Host;
    comm.ReconnectTimeout = null; //TimeSpan.FromSeconds(30);
    comm.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

    comm.ReconnectionHappened.Subscribe(info =>
        Log.Information("[{relay}] Reconnection happened, type: {type}", comm.Name, info.Type));
    comm.DisconnectionHappened.Subscribe(info =>
        Log.Information("[{relay}] Disconnection happened, type: {type}, reason: {reason}", comm.Name, info.Type, info.CloseStatus));
    return comm;
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

void SendEvent(INostrClient client, int counter)
{
    var ev = new NostrEvent
    {
        Kind = NostrKind.ShortTextNote,
        CreatedAt = DateTime.UtcNow,
        Content = $"Test message {counter} from C# client"
    };

    var key = NostrPrivateKey.FromBech32("nsec1xjyhgzm2cjv2wp64wnh64d2n4s9ylguhwelekh5r38rlsfgk6mes62duaa");
    var signed = ev.Sign(key);

    client.Send(new NostrEventRequest(signed));
}

void SendDirectMessage(INostrClient client)
{
    Log.Information("Sending encrypted direct message");

    var sender = NostrPrivateKey.FromBech32("nsec1l0a7m5dlg4h9wurhnmgsq5nv9cqyvdwsutk4yf3w4fzzaqw7n80ssdfzkg");
    var receiver = NostrPublicKey.FromHex("d27790fcb3f9afa0d709b2e9c5995151bc5ad008079bd0a474aa101d80e0eed3");

    var ev = new NostrEvent
    {
        CreatedAt = DateTime.UtcNow,
        Content = $"Test private message from C# client"
    };

    var encrypted = ev.EncryptDirect(sender, receiver);
    var signed = encrypted.Sign(sender);

    client.Send(new NostrEventRequest(signed));
}
