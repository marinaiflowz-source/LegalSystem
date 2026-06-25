using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class add_MaincaseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainCase",
                table: "TblCases",
                newName: "MainCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_MainCaseId",
                table: "TblCases",
                column: "MainCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCases_TblCases_MainCaseId",
                table: "TblCases",
                column: "MainCaseId",
                principalTable: "TblCases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCases_TblCases_MainCaseId",
                table: "TblCases");

            migrationBuilder.DropIndex(
                name: "IX_TblCases_MainCaseId",
                table: "TblCases");

            migrationBuilder.RenameColumn(
                name: "MainCaseId",
                table: "TblCases",
                newName: "MainCase");
        }
    }
}
