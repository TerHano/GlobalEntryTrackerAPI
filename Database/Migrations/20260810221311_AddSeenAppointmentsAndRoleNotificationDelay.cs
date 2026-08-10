using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSeenAppointmentsAndRoleNotificationDelay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "(now() at time zone 'utc')",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2026, 2, 26, 1, 42, 7, 336, DateTimeKind.Utc).AddTicks(3310));

            migrationBuilder.AddColumn<int>(
                name: "NotificationDelayInMinutes",
                table: "AspNetRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SeenAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    StartTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeenAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeenAppointments_AppointmentLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "AppointmentLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeenAppointments_LocationId_StartTimestamp",
                table: "SeenAppointments",
                columns: new[] { "LocationId", "StartTimestamp" },
                unique: true);

            // Tier separation now comes from the release delay rather than from a long cooldown, so
            // the free cooldown drops at the same time. Paid roles keep 0 delay (the column
            // default). Keep in step with RoleSeederService.GetRoles().
            migrationBuilder.Sql(
                """
                UPDATE "AspNetRoles"
                SET "NotificationDelayInMinutes" = 45,
                    "NotificationIntervalInMinutes" = 120
                WHERE "Code" = 'free';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "AspNetRoles"
                SET "NotificationIntervalInMinutes" = 300
                WHERE "Code" = 'free';
                """);

            migrationBuilder.DropTable(
                name: "SeenAppointments");

            migrationBuilder.DropColumn(
                name: "NotificationDelayInMinutes",
                table: "AspNetRoles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2026, 2, 26, 1, 42, 7, 336, DateTimeKind.Utc).AddTicks(3310),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "(now() at time zone 'utc')");
        }
    }
}
