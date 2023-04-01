using Microsoft.EntityFrameworkCore;

namespace NostrBot.Web.Storage;

public class BotContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public BotContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public DbSet<ProcessedEvent> ProcessedEvents { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(Configuration.GetConnectionString("BotDatabase"));
    }
}
