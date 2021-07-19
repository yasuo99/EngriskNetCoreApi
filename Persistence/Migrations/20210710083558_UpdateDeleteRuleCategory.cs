using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class UpdateDeleteRuleCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_WordCategories_WordCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Word_WordId",
                table: "Categories");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_WordCategories_WordCategoryId",
                table: "Categories",
                column: "WordCategoryId",
                principalTable: "WordCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Word_WordId",
                table: "Categories",
                column: "WordId",
                principalTable: "Word",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_WordCategories_WordCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Word_WordId",
                table: "Categories");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_WordCategories_WordCategoryId",
                table: "Categories",
                column: "WordCategoryId",
                principalTable: "WordCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Word_WordId",
                table: "Categories",
                column: "WordId",
                principalTable: "Word",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
