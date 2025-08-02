using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class fixdatabasemore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_DiscordNotifications_DiscordNotificationS~",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_EmailNotificationSettings_EmailNotificati~",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_DiscordNotificationSettingsId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_EmailNotificationSettingsId",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordNotifications",
                table: "DiscordNotifications");

            migrationBuilder.RenameTable(
                name: "DiscordNotifications",
                newName: "DiscordNotificationSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 8, 2, 5, 38, 31, 589, DateTimeKind.Utc).AddTicks(2140),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 8, 2, 5, 14, 23, 235, DateTimeKind.Utc).AddTicks(8750));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordNotificationSettings",
                table: "DiscordNotificationSettings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationSettings_UserNotificationId",
                table: "EmailNotificationSettings",
                column: "UserNotificationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordNotificationSettings_UserNotificationId",
                table: "DiscordNotificationSettings",
                column: "UserNotificationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordNotificationSettings_UserNotifications_UserNotificat~",
                table: "DiscordNotificationSettings",
                column: "UserNotificationId",
                principalTable: "UserNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotificationSettings_UserNotifications_UserNotificatio~",
                table: "EmailNotificationSettings",
                column: "UserNotificationId",
                principalTable: "UserNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordNotificationSettings_UserNotifications_UserNotificat~",
                table: "DiscordNotificationSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotificationSettings_UserNotifications_UserNotificatio~",
                table: "EmailNotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotificationSettings_UserNotificationId",
                table: "EmailNotificationSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordNotificationSettings",
                table: "DiscordNotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_DiscordNotificationSettings_UserNotificationId",
                table: "DiscordNotificationSettings");

            migrationBuilder.RenameTable(
                name: "DiscordNotificationSettings",
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
                oldDefaultValue: new DateTime(2025, 8, 2, 5, 38, 31, 589, DateTimeKind.Utc).AddTicks(2140));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordNotifications",
                table: "DiscordNotifications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_DiscordNotificationSettingsId",
                table: "UserNotifications",
                column: "DiscordNotificationSettingsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_EmailNotificationSettingsId",
                table: "UserNotifications",
                column: "EmailNotificationSettingsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_DiscordNotifications_DiscordNotificationS~",
                table: "UserNotifications",
                column: "DiscordNotificationSettingsId",
                principalTable: "DiscordNotifications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_EmailNotificationSettings_EmailNotificati~",
                table: "UserNotifications",
                column: "EmailNotificationSettingsId",
                principalTable: "EmailNotificationSettings",
                principalColumn: "Id");
        }
    }
}
