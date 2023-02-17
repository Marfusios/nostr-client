using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Logging;
using Nostr.Client.Client;
using Nostr.Client.Communicator;
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

// var url = new Uri("wss://relay.snort.social");
// var url = new Uri("wss://relay.damus.io");
// var url = new Uri("wss://eden.nostr.land");
var url = new Uri("wss://nostr-pub.wellorder.net");

using var communicator = new NostrWebsocketCommunicator(url, () =>
{
    var client = new ClientWebSocket();
    client.Options.SetRequestHeader("Origin", "http://localhost");
    return client;
});

communicator.Name = $"Nostr {url.Host}";
communicator.ReconnectTimeout = null; //TimeSpan.FromSeconds(30);
communicator.ErrorReconnectTimeout = TimeSpan.FromSeconds(60);

communicator.ReconnectionHappened.Subscribe(info =>
    Log.Information($"Reconnection happened, type: {info.Type}"));
communicator.DisconnectionHappened.Subscribe(info =>
    Log.Information($"Disconnection happened, type: {info.Type}, reason: {info.CloseStatusDescription}"));

using var client = new NostrWebsocketClient(communicator, logFactory.CreateLogger<NostrWebsocketClient>());
var viewer = new NostrViewer(client);

viewer.Subscribe();
await communicator.StartOrFail();

Log.Debug("Sending requests");
viewer.SendRequests();

exitEvent.WaitOne();

Log.Debug("====================================");
Log.Debug("              STOPPING              ");
Log.Debug("====================================");
Log.CloseAndFlush();

await communicator.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
await Task.Delay(TimeSpan.FromSeconds(1));

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