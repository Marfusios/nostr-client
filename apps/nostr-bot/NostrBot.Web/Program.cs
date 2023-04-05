using Nostr.Client.Client;
using NostrBot.Web.Configs;
using NostrBot.Web.Logic;
using NostrBot.Web.Utils;
using Serilog;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Nostr.Client.Responses;
using NostrBot.Web.Storage;
using OpenAI;

namespace NostrBot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .AddUserSecrets<Program>(true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Configuring web application");
                var builder = WebApplication.CreateBuilder(args);
                var services = builder.Services;

                builder.Host.UseSerilog();

                configuration.Configure<BotConfig>(services, "bot");
                configuration.Configure<NostrConfig>(services, "nostr");
                var openAiConfig = configuration.Configure<OpenAiConfig>(services, "openai");

                services.AddControllers();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();

                services.AddDbContext<BotContext>();
                services.AddSingleton<BotStorage>();
                services.AddSingleton(_ => new OpenAIClient(new OpenAIAuthentication(openAiConfig.ApiKey, openAiConfig.Organization)));

                services.AddSingleton<NostrMultiWebsocketClient>();
                services.AddSingleton<NostrListener>();
                services.AddSingleton<BotMind>();
                services.AddSingleton<BotManagement>();

                services.AddSingleton(_ => new NostrEventsQueue(
                    Channel.CreateUnbounded<NostrEventResponse>(new UnboundedChannelOptions
                    {
                        SingleReader = true,
                        SingleWriter = false
                    })));

                services.AddHostedService<BackgroundOrchestration>();
                services.AddHostedService<BotMind>();

                var app = builder.Build();

                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                InitStorage(app);

                Log.Information("Starting web application");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly, error: {error}", ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void InitStorage(WebApplication app)
        {
            Log.Information("Initializing database storage");

            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            db.Database.Migrate();
        }
    }
}