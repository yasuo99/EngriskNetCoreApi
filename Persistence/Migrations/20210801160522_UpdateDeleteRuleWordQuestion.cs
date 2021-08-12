using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class UpdateDeleteRuleWordQuestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WordQuestions_Questions_QuestionId",
                table: "WordQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_WordQuestions_Word_WordId",
                table: "WordQuestions");

            migrationBuilder.AddForeignKey(
                name: "FK_WordQuestions_Questions_QuestionId",
                table: "WordQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WordQuestions_Word_WordId",
                table: "WordQuestions",
                column: "WordId",
                principalTable: "Word",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WordQuestions_Questions_QuestionId",
                table: "WordQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_WordQuestions_Word_WordId",
                table: "WordQuestions");

            migrationBuilder.AddForeignKey(
                name: "FK_WordQuestions_Questions_QuestionId",
                table: "WordQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WordQuestions_Word_WordId",
                table: "WordQuestions",
                column: "WordId",
                principalTable: "Word",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
