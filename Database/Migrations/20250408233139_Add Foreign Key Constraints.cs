using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTrackedLocations_LocationId",
                table: "UserTrackedLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrackedLocations_UserId",
                table: "UserTrackedLocations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrackedLocations_AppointmentLocations_LocationId",
                table: "UserTrackedLocations",
                column: "LocationId",
                principalTable: "AppointmentLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrackedLocations_Users_UserId",
                table: "UserTrackedLocations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTrackedLocations_AppointmentLocations_LocationId",
                table: "UserTrackedLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTrackedLocations_Users_UserId",
                table: "UserTrackedLocations");

            migrationBuilder.DropIndex(
                name: "IX_UserTrackedLocations_LocationId",
                table: "UserTrackedLocations");

            migrationBuilder.DropIndex(
                name: "IX_UserTrackedLocations_UserId",
                table: "UserTrackedLocations");
        }
    }
}
