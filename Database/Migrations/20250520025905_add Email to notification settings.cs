using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class addEmailtonotificationsettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 20, 2, 59, 5, 356, DateTimeKind.Utc).AddTicks(4790),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 19, 7, 0, 2, 433, DateTimeKind.Utc).AddTicks(6940));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EmailNotificationSettings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "EmailNotificationSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 19, 7, 0, 2, 433, DateTimeKind.Utc).AddTicks(6940),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 20, 2, 59, 5, 356, DateTimeKind.Utc).AddTicks(4790));
        }
    }
}
