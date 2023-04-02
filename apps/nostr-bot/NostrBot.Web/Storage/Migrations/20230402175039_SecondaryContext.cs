using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NostrBot.Web.Storage.Migrations
{
    /// <inheritdoc />
    public partial class SecondaryContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReplySecondaryContextId",
                table: "ProcessedEvents",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplySecondaryContextId",
                table: "ProcessedEvents");
        }
    }
}
