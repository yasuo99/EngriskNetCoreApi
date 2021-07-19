using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class AddCategoryTagModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WordCategories",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsToeicVocabulary",
                table: "WordCategories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CategoryTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordCategoryTags",
                columns: table => new
                {
                    WordCategoryId = table.Column<Guid>(nullable: false),
                    CategoryTagId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordCategoryTags", x => new { x.WordCategoryId, x.CategoryTagId });
                    table.ForeignKey(
                        name: "FK_WordCategoryTags_CategoryTags_CategoryTagId",
                        column: x => x.CategoryTagId,
                        principalTable: "CategoryTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WordCategoryTags_WordCategories_WordCategoryId",
                        column: x => x.WordCategoryId,
                        principalTable: "WordCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WordCategoryTags_CategoryTagId",
                table: "WordCategoryTags",
                column: "CategoryTagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordCategoryTags");

            migrationBuilder.DropTable(
                name: "CategoryTags");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WordCategories");

            migrationBuilder.DropColumn(
                name: "IsToeicVocabulary",
                table: "WordCategories");
        }
    }
}
