using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class Addappointmentarchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 20, 3, 30, 9, 140, DateTimeKind.Utc).AddTicks(2720),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 20, 2, 59, 5, 356, DateTimeKind.Utc).AddTicks(4790));

            migrationBuilder.CreateTable(
                name: "ArchivedAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    StartTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivedAppointments_AppointmentLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "AppointmentLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedAppointments_LocationId",
                table: "ArchivedAppointments",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedAppointments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 5, 20, 2, 59, 5, 356, DateTimeKind.Utc).AddTicks(4790),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 5, 20, 3, 30, 9, 140, DateTimeKind.Utc).AddTicks(2720));
        }
    }
}
