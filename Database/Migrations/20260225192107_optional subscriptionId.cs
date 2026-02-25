using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class optionalsubscriptionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2026, 2, 25, 19, 21, 6, 704, DateTimeKind.Utc).AddTicks(4800),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2026, 2, 23, 7, 4, 3, 695, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.AlterColumn<string>(
                name: "SubscriptionId",
                table: "UserCustomers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2026, 2, 23, 7, 4, 3, 695, DateTimeKind.Utc).AddTicks(7300),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2026, 2, 25, 19, 21, 6, 704, DateTimeKind.Utc).AddTicks(4800));

            migrationBuilder.AlterColumn<string>(
                name: "SubscriptionId",
                table: "UserCustomers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
