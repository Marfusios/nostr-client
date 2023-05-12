using Microsoft.EntityFrameworkCore;
using Nostr.Client.Messages;
using Nostr.Client.Responses;

namespace NostrBot.Web.Storage
{
    public class BotStorage
    {
        private readonly IDbContextFactory<BotContext> _contextFactory;

        public BotStorage(IDbContextFactory<BotContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> IsProcessed(NostrEvent ev)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ProcessedEvents
                .AsNoTracking()
                .AnyAsync(x => x.NostrEventId == ev.Id);
        }

        public async Task Store(string contextId, NostrEventResponse response, NostrEvent ev, string? generatedReply, string? eventContent, string? secondaryContextId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

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
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProcessedEvents
                .AsNoTracking()
                .Where(x => x.ReplyContextId == contextId || (x.ReplySecondaryContextId != null && x.ReplySecondaryContextId == secondaryContextId))
                .OrderByDescending(x => x.Created)
                .Take(33)
                .ToArrayAsync();
        }
    }
}
