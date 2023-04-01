using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NostrBot.Web.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedEvents",
                columns: table => new
                {
                    ProcessedEventId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Subscription = table.Column<string>(type: "TEXT", nullable: true),
                    Relay = table.Column<string>(type: "TEXT", nullable: false),
                    NostrEventId = table.Column<string>(type: "TEXT", nullable: true),
                    NostrEventContent = table.Column<string>(type: "TEXT", nullable: true),
                    NostrEventPubkey = table.Column<string>(type: "TEXT", nullable: true),
                    NostrEventKind = table.Column<int>(type: "INTEGER", nullable: false),
                    NostrEventCreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NostrEventTagP = table.Column<string>(type: "TEXT", nullable: true),
                    NostrEventTagE = table.Column<string>(type: "TEXT", nullable: true),
                    ReplyContextId = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratedReply = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedEvents", x => x.ProcessedEventId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedEvents");
        }
    }
}
