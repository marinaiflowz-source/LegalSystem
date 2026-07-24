using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_AssignUser_CaseTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AssignedUserId",
                table: "TblCases",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_AssignedUserId",
                table: "TblCases",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCases_TblUsers_AssignedUserId",
                table: "TblCases",
                column: "AssignedUserId",
                principalTable: "TblUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCases_TblUsers_AssignedUserId",
                table: "TblCases");

            migrationBuilder.DropIndex(
                name: "IX_TblCases_AssignedUserId",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "TblCases");
        }
    }
}
