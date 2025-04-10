using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotificationType",
                table: "UserTrackedLocations",
                newName: "NotificationTypeId");

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTrackedLocations_NotificationTypeId",
                table: "UserTrackedLocations",
                column: "NotificationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrackedLocations_NotificationTypes_NotificationTypeId",
                table: "UserTrackedLocations",
                column: "NotificationTypeId",
                principalTable: "NotificationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTrackedLocations_NotificationTypes_NotificationTypeId",
                table: "UserTrackedLocations");

            migrationBuilder.DropTable(
                name: "NotificationTypes");

            migrationBuilder.DropIndex(
                name: "IX_UserTrackedLocations_NotificationTypeId",
                table: "UserTrackedLocations");

            migrationBuilder.RenameColumn(
                name: "NotificationTypeId",
                table: "UserTrackedLocations",
                newName: "NotificationType");
        }
    }
}
