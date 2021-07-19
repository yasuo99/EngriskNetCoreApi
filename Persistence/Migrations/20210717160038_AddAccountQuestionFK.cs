using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class AddAccountQuestionFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Questions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AccountId",
                table: "Questions",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_AspNetUsers_AccountId",
                table: "Questions",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_AspNetUsers_AccountId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_AccountId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Questions");
        }
    }
}
