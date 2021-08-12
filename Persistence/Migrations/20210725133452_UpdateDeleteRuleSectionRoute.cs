using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class UpdateDeleteRuleSectionRoute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Routes_RouteId",
                table: "Sections");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Routes_RouteId",
                table: "Sections",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Routes_RouteId",
                table: "Sections");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Routes_RouteId",
                table: "Sections",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
