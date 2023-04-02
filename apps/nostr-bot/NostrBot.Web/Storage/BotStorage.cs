using Microsoft.EntityFrameworkCore;
using Nostr.Client.Messages;
using Nostr.Client.Responses;

namespace NostrBot.Web.Storage
{
    public class BotStorage
    {
        private readonly IServiceProvider _serviceProvider;

        public BotStorage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> IsProcessed(NostrEvent ev)
        {
            using var _ = ResolveContext(out var context);

            return await context.ProcessedEvents
                .AnyAsync(x => x.NostrEventId == ev.Id);
        }

        public async Task Store(string contextId, NostrEventResponse response, NostrEvent ev, string? generatedReply, string? eventContent, string? secondaryContextId)
        {
            using var _ = ResolveContext(out var context);

            var processedEvent = new ProcessedEvent
            {
                Created = DateTime.UtcNow,
                Subscription = response.Subscription,
                Relay = response.CommunicatorName,
                NostrEventId = ev.Id,
                NostrEventContent = eventContent,
                NostrEventPubkey = ev.Pubkey,
                NostrEventCreatedAt = ev.CreatedAt,
                NostrEventKind = ev.Kind,
                NostrEventTagP = ev.Tags?.FindFirstTagValue(NostrEventTag.ProfileIdentifier),
                NostrEventTagE = ev.Tags?.FindFirstTagValue(NostrEventTag.EventIdentifier),
                ReplyContextId = contextId,
                ReplySecondaryContextId = secondaryContextId,
                GeneratedReply = generatedReply
            };

            context.ProcessedEvents.Add(processedEvent);
            await context.SaveChangesAsync();
        }

        public async Task<ProcessedEvent[]> GetHistoryForContext(string contextId, string? secondaryContextId)
        {
            using var _ = ResolveContext(out var context);
            return await context.ProcessedEvents
                .Where(x => x.ReplyContextId == contextId || x.ReplySecondaryContextId == secondaryContextId)
                .OrderByDescending(x => x.Created)
                .ToArrayAsync();
        }

        private IDisposable ResolveContext(out BotContext context)
        {
            var scope = _serviceProvider.CreateScope();
            context = scope.ServiceProvider.GetRequiredService<BotContext>();
            return scope;
        }
    }
}
