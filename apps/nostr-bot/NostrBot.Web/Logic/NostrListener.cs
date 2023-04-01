using Nostr.Client.Client;
using Nostr.Client.Communicator;
using NostrBot.Web.Configs;
using Serilog;
using System.Net.WebSockets;
using Microsoft.Extensions.Options;
using Nostr.Client.Requests;
using Websocket.Client.Models;

namespace NostrBot.Web.Logic;

public class NostrListener : IDisposable
{
    private readonly NostrConfig _config;
    private readonly NostrMultiWebsocketClient _client;
    private readonly INostrCommunicator[] _communicators;

    private readonly List<NostrFilter> _filters = new();

    public NostrListener(IOptions<NostrConfig> config, NostrMultiWebsocketClient client)
    {
        _config = config.Value;
        _client = client;

        _communicators = CreateCommunicators();
        foreach (var communicator in _communicators)
            _client.RegisterCommunicator(communicator);
    }

    public NostrClientStreams Streams => _client.Streams;

    public void Dispose()
    {
        _client.Dispose();

        foreach (var comm in _communicators)
        {
            comm.Dispose();
        }
    }

    public void RegisterFilter(NostrFilter filter)
    {
        _filters.Add(filter);
    }

    public void Start()
    {
        foreach (var comm in _communicators)
        {
            // fire and forget
            _ = comm.Start();
        }
    }

    public void Stop()
    {
        foreach (var comm in _communicators)
        {
            // fire and forget
            _ = comm.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
        }
    }

    private INostrCommunicator[] CreateCommunicators() =>
        _config.Relays
            .Select(x => CreateCommunicator(new Uri(x)))
            .ToArray();

    private INostrCommunicator CreateCommunicator(Uri uri)
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

        comm.ReconnectionHappened.Subscribe(info => OnCommunicatorReconnection(info, comm.Name));
        comm.DisconnectionHappened.Subscribe(info =>
            Log.Information("[{relay}] Disconnected, type: {type}, reason: {reason}", comm.Name, info.Type, info.CloseStatus));
        return comm;
    }

    private void OnCommunicatorReconnection(ReconnectionInfo info, string communicatorName)
    {
        try
        {
            Log.Information("[{relay}] Reconnected, sending Nostr filters ({filterCount})", communicatorName, _filters.Count);

            var client = _client.FindClient(communicatorName);
            if (client == null)
            {
                Log.Warning("[{relay}] Cannot find client", communicatorName);
                return;
            }

            var counter = 0;
            foreach (var filter in _filters)
            {
                client.Send(new NostrRequest($"bot:data:{counter}", filter));
                counter++;
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{relay}] Failed to process reconnection, error: {error}", communicatorName, e.Message);
        }
    }
}
