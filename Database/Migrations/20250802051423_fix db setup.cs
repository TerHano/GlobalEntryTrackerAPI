using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class fixdbsetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_DiscordNotificationSettingsEntity_Discord~",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordNotificationSettingsEntity",
                table: "DiscordNotificationSettingsEntity");

            migrationBuilder.RenameTable(
                name: "DiscordNotificationSettingsEntity",
                newName: "DiscordNotifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 8, 2, 5, 14, 23, 235, DateTimeKind.Utc).AddTicks(8750),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 8, 1, 19, 31, 10, 822, DateTimeKind.Utc).AddTicks(1370));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordNotifications",
                table: "DiscordNotifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_DiscordNotifications_DiscordNotificationS~",
                table: "UserNotifications",
                column: "DiscordNotificationSettingsId",
                principalTable: "DiscordNotifications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_DiscordNotifications_DiscordNotificationS~",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordNotifications",
                table: "DiscordNotifications");

            migrationBuilder.RenameTable(
                name: "DiscordNotifications",
                newName: "DiscordNotificationSettingsEntity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 8, 1, 19, 31, 10, 822, DateTimeKind.Utc).AddTicks(1370),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 8, 2, 5, 14, 23, 235, DateTimeKind.Utc).AddTicks(8750));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordNotificationSettingsEntity",
                table: "DiscordNotificationSettingsEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_DiscordNotificationSettingsEntity_Discord~",
                table: "UserNotifications",
                column: "DiscordNotificationSettingsId",
                principalTable: "DiscordNotificationSettingsEntity",
                principalColumn: "Id");
        }
    }
}
