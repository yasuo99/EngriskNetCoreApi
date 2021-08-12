using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class updateExamAndQuizModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PauseQuestion",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "CurrentQuestion",
                table: "ExamHistories");

            migrationBuilder.DropColumn(
                name: "Timestamp_pause",
                table: "ExamHistories");

            migrationBuilder.AddColumn<int>(
                name: "Listening",
                table: "ExamHistories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Reading",
                table: "ExamHistories",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Listening",
                table: "ExamHistories");

            migrationBuilder.DropColumn(
                name: "Reading",
                table: "ExamHistories");

            migrationBuilder.AddColumn<int>(
                name: "PauseQuestion",
                table: "Histories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentQuestion",
                table: "ExamHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp_pause",
                table: "ExamHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
