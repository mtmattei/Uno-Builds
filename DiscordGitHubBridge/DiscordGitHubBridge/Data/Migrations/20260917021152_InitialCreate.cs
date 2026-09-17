using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordGitHubBridge.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThreadIssueMappings",
                columns: table => new
                {
                    DiscordThreadId = table.Column<long>(type: "INTEGER", nullable: false),
                    DiscordChannelId = table.Column<long>(type: "INTEGER", nullable: false),
                    GitHubIssueNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    GitHubIssueUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadIssueMappings", x => x.DiscordThreadId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreadIssueMappings_Status",
                table: "ThreadIssueMappings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreadIssueMappings");
        }
    }
}
