using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class ChangeCeritificateFKInScript : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Routes_RouteId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_RouteId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Certificates");

            migrationBuilder.AddColumn<Guid>(
                name: "ScriptId",
                table: "Certificates",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ScriptId",
                table: "Certificates",
                column: "ScriptId",
                unique: true,
                filter: "[ScriptId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Scripts_ScriptId",
                table: "Certificates",
                column: "ScriptId",
                principalTable: "Scripts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Scripts_ScriptId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_ScriptId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ScriptId",
                table: "Certificates");

            migrationBuilder.AddColumn<Guid>(
                name: "RouteId",
                table: "Certificates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_RouteId",
                table: "Certificates",
                column: "RouteId",
                unique: true,
                filter: "[RouteId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Routes_RouteId",
                table: "Certificates",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
