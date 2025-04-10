using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class DiscordNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscordNotificationSettingsId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiscordNotificationSettingsEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WebhookUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordNotificationSettingsEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DiscordNotificationSettingsId",
                table: "Users",
                column: "DiscordNotificationSettingsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_DiscordNotificationSettingsEntity_DiscordNotification~",
                table: "Users",
                column: "DiscordNotificationSettingsId",
                principalTable: "DiscordNotificationSettingsEntity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_DiscordNotificationSettingsEntity_DiscordNotification~",
                table: "Users");

            migrationBuilder.DropTable(
                name: "DiscordNotificationSettingsEntity");

            migrationBuilder.DropIndex(
                name: "IX_Users_DiscordNotificationSettingsId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscordNotificationSettingsId",
                table: "Users");
        }
    }
}
