using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class removeuserIdanduseusernotificationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordNotificationSettingsEntity_Users_UserId",
                table: "DiscordNotificationSettingsEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotificationSettings_Users_UserId",
                table: "EmailNotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_DiscordNotificationSettingsId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_EmailNotificationSettingsId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotificationSettings_UserId",
                table: "EmailNotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_DiscordNotificationSettingsEntity_UserId",
                table: "DiscordNotificationSettingsEntity");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EmailNotificationSettings",
                newName: "UserNotificationId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DiscordNotificationSettingsEntity",
                newName: "UserNotificationId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 19, 2, 22, 46, 163, DateTimeKind.Utc).AddTicks(7880),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 19, 1, 14, 14, 680, DateTimeKind.Utc).AddTicks(4820));

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

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_DiscordNotificationSettingsId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_EmailNotificationSettingsId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications");

            migrationBuilder.RenameColumn(
                name: "UserNotificationId",
                table: "EmailNotificationSettings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserNotificationId",
                table: "DiscordNotificationSettingsEntity",
                newName: "UserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 19, 1, 14, 14, 680, DateTimeKind.Utc).AddTicks(4820),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 19, 2, 22, 46, 163, DateTimeKind.Utc).AddTicks(7880));

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

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationSettings_UserId",
                table: "EmailNotificationSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordNotificationSettingsEntity_UserId",
                table: "DiscordNotificationSettingsEntity",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordNotificationSettingsEntity_Users_UserId",
                table: "DiscordNotificationSettingsEntity",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotificationSettings_Users_UserId",
                table: "EmailNotificationSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
