using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class ChangeAccountCertificateKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountCertificates",
                table: "AccountCertificates");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "AccountCertificates",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountCertificates",
                table: "AccountCertificates",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountCertificates_AccountId",
                table: "AccountCertificates",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountCertificates",
                table: "AccountCertificates");

            migrationBuilder.DropIndex(
                name: "IX_AccountCertificates_AccountId",
                table: "AccountCertificates");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AccountCertificates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountCertificates",
                table: "AccountCertificates",
                columns: new[] { "AccountId", "CertificateId" });
        }
    }
}
