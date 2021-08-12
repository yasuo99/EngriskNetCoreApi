using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class UpdateDeleteRuleScriptWord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScriptWords_Word_WordId",
                table: "ScriptWords");

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptWords_Word_WordId",
                table: "ScriptWords",
                column: "WordId",
                principalTable: "Word",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScriptWords_Word_WordId",
                table: "ScriptWords");

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptWords_Word_WordId",
                table: "ScriptWords",
                column: "WordId",
                principalTable: "Word",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
