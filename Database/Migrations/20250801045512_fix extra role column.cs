using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class fixextrarolecolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserEntityId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserEntityId",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "UserEntityId",
                table: "AspNetUserRoles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 8, 1, 4, 55, 11, 762, DateTimeKind.Utc).AddTicks(880),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 8, 1, 4, 47, 29, 166, DateTimeKind.Utc).AddTicks(9860));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NextNotificationAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 8, 1, 4, 47, 29, 166, DateTimeKind.Utc).AddTicks(9860),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 8, 1, 4, 55, 11, 762, DateTimeKind.Utc).AddTicks(880));

            migrationBuilder.AddColumn<string>(
                name: "UserEntityId",
                table: "AspNetUserRoles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserEntityId",
                table: "AspNetUserRoles",
                column: "UserEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserEntityId",
                table: "AspNetUserRoles",
                column: "UserEntityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
