using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class EditDeleteRuleAccountAnswer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountAnswers_Answers_AnswerId",
                table: "AccountAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountAnswers_Answers_AnswerId",
                table: "AccountAnswers",
                column: "AnswerId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountAnswers_Answers_AnswerId",
                table: "AccountAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountAnswers_Answers_AnswerId",
                table: "AccountAnswers",
                column: "AnswerId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
