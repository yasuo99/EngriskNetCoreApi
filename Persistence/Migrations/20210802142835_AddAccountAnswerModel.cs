using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class AddAccountAnswerModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndPage",
                table: "Exam",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartPage",
                table: "Exam",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AccountAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExamHistoryId = table.Column<Guid>(nullable: false),
                    QuestionId = table.Column<Guid>(nullable: false),
                    AnswerId = table.Column<Guid>(nullable: false),
                    Result = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountAnswers_Answers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountAnswers_ExamHistories_ExamHistoryId",
                        column: x => x.ExamHistoryId,
                        principalTable: "ExamHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAnswers_AnswerId",
                table: "AccountAnswers",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountAnswers_ExamHistoryId",
                table: "AccountAnswers",
                column: "ExamHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountAnswers_QuestionId",
                table: "AccountAnswers",
                column: "QuestionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAnswers");

            migrationBuilder.DropColumn(
                name: "EndPage",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "StartPage",
                table: "Exam");
        }
    }
}
