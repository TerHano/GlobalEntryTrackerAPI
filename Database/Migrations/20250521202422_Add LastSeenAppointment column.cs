using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSeenAppointmentcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenEarliestAppointment",
                table: "UserTrackedLocations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 21, 20, 24, 22, 292, DateTimeKind.Utc).AddTicks(9480),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 20, 3, 30, 9, 140, DateTimeKind.Utc).AddTicks(2720));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenEarliestAppointment",
                table: "UserTrackedLocations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 20, 3, 30, 9, 140, DateTimeKind.Utc).AddTicks(2720),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 21, 20, 24, 22, 292, DateTimeKind.Utc).AddTicks(9480));
        }
    }
}
