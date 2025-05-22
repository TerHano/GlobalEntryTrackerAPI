using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class Onlycapturedateinarchivetoreducedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTimestamp",
                table: "ArchivedAppointments");

            migrationBuilder.DropColumn(
                name: "StartTimestamp",
                table: "ArchivedAppointments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 22, 4, 54, 20, 526, DateTimeKind.Utc).AddTicks(9020),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 21, 20, 24, 22, 292, DateTimeKind.Utc).AddTicks(9480));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "ArchivedAppointments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "ArchivedAppointments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 21, 20, 24, 22, 292, DateTimeKind.Utc).AddTicks(9480),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 22, 4, 54, 20, 526, DateTimeKind.Utc).AddTicks(9020));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTimestamp",
                table: "ArchivedAppointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTimestamp",
                table: "ArchivedAppointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
