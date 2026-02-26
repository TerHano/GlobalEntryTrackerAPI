using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyNotificationLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2026, 2, 26, 1, 42, 7, 336, DateTimeKind.Utc).AddTicks(3310),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2026, 2, 25, 19, 21, 6, 704, DateTimeKind.Utc).AddTicks(4800));

            migrationBuilder.AddColumn<int>(
                name: "DailyNotificationCount",
                table: "EmailNotificationSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DailyNotificationWindowStart",
                table: "EmailNotificationSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxNotificationsPerDay",
                table: "EmailNotificationSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyNotificationCount",
                table: "DiscordNotificationSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DailyNotificationWindowStart",
                table: "DiscordNotificationSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxNotificationsPerDay",
                table: "DiscordNotificationSettings",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyNotificationCount",
                table: "EmailNotificationSettings");

            migrationBuilder.DropColumn(
                name: "DailyNotificationWindowStart",
                table: "EmailNotificationSettings");

            migrationBuilder.DropColumn(
                name: "MaxNotificationsPerDay",
                table: "EmailNotificationSettings");

            migrationBuilder.DropColumn(
                name: "DailyNotificationCount",
                table: "DiscordNotificationSettings");

            migrationBuilder.DropColumn(
                name: "DailyNotificationWindowStart",
                table: "DiscordNotificationSettings");

            migrationBuilder.DropColumn(
                name: "MaxNotificationsPerDay",
                table: "DiscordNotificationSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2026, 2, 25, 19, 21, 6, 704, DateTimeKind.Utc).AddTicks(4800),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2026, 2, 26, 1, 42, 7, 336, DateTimeKind.Utc).AddTicks(3310));
        }
    }
}
