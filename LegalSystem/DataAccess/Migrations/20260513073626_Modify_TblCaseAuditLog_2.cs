using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Modify_TblCaseAuditLog_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseAuditLog_TblCases_CaseId",
                table: "TblCaseAuditLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TblCaseAuditLog",
                table: "TblCaseAuditLog");

            migrationBuilder.RenameTable(
                name: "TblCaseAuditLog",
                newName: "TblCaseAuditLogs");

            migrationBuilder.RenameIndex(
                name: "IX_TblCaseAuditLog_CaseId",
                table: "TblCaseAuditLogs",
                newName: "IX_TblCaseAuditLogs_CaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TblCaseAuditLogs",
                table: "TblCaseAuditLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseAuditLogs_TblCases_CaseId",
                table: "TblCaseAuditLogs",
                column: "CaseId",
                principalTable: "TblCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseAuditLogs_TblCases_CaseId",
                table: "TblCaseAuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TblCaseAuditLogs",
                table: "TblCaseAuditLogs");

            migrationBuilder.RenameTable(
                name: "TblCaseAuditLogs",
                newName: "TblCaseAuditLog");

            migrationBuilder.RenameIndex(
                name: "IX_TblCaseAuditLogs_CaseId",
                table: "TblCaseAuditLog",
                newName: "IX_TblCaseAuditLog_CaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TblCaseAuditLog",
                table: "TblCaseAuditLog",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseAuditLog_TblCases_CaseId",
                table: "TblCaseAuditLog",
                column: "CaseId",
                principalTable: "TblCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
