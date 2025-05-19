using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class Seperateusernotificationstonewtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_DiscordNotificationSettingsEntity_DiscordNotification~",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DiscordNotificationSettingsId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscordNotificationSettingsId",
                table: "Users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 19, 1, 14, 14, 680, DateTimeKind.Utc).AddTicks(4820),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 19, 0, 53, 41, 276, DateTimeKind.Utc).AddTicks(6230));

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DiscordNotificationSettingsId = table.Column<int>(type: "integer", nullable: true),
                    EmailNotificationSettingsId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_DiscordNotificationSettingsEntity_Discord~",
                        column: x => x.DiscordNotificationSettingsId,
                        principalTable: "DiscordNotificationSettingsEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNotifications_EmailNotificationSettings_EmailNotificati~",
                        column: x => x.EmailNotificationSettingsId,
                        principalTable: "EmailNotificationSettings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordNotificationSettingsEntity_UserId",
                table: "DiscordNotificationSettingsEntity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_DiscordNotificationSettingsId",
                table: "UserNotifications",
                column: "DiscordNotificationSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_EmailNotificationSettingsId",
                table: "UserNotifications",
                column: "EmailNotificationSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordNotificationSettingsEntity_Users_UserId",
                table: "DiscordNotificationSettingsEntity",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordNotificationSettingsEntity_Users_UserId",
                table: "DiscordNotificationSettingsEntity");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_DiscordNotificationSettingsEntity_UserId",
                table: "DiscordNotificationSettingsEntity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 19, 0, 53, 41, 276, DateTimeKind.Utc).AddTicks(6230),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 19, 1, 14, 14, 680, DateTimeKind.Utc).AddTicks(4820));

            migrationBuilder.AddColumn<int>(
                name: "DiscordNotificationSettingsId",
                table: "Users",
                type: "integer",
                nullable: true);

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
    }
}
