using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class UpdateNullablePropAccountAnswer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountAnswers_ExamHistories_ExamHistoryId",
                table: "AccountAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExamHistoryId",
                table: "AccountAnswers",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountAnswers_ExamHistories_ExamHistoryId",
                table: "AccountAnswers",
                column: "ExamHistoryId",
                principalTable: "ExamHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountAnswers_ExamHistories_ExamHistoryId",
                table: "AccountAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExamHistoryId",
                table: "AccountAnswers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountAnswers_ExamHistories_ExamHistoryId",
                table: "AccountAnswers",
                column: "ExamHistoryId",
                principalTable: "ExamHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
