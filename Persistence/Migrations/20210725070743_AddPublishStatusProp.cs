using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class AddPublishStatusProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "Word",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "Sections",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "Routes",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "Quiz",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "Exam",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Word");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Exam");
        }
    }
}
