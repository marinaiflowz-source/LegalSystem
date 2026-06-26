using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaseTbl_RemoveLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCases_RefCaseLevels_LevelId",
                table: "TblCases");

            migrationBuilder.DropIndex(
                name: "IX_TblCases_LevelId",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "TblCases");

            migrationBuilder.AddColumn<int>(
                name: "RefCaseLevelId",
                table: "TblCases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_RefCaseLevelId",
                table: "TblCases",
                column: "RefCaseLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCases_RefCaseLevels_RefCaseLevelId",
                table: "TblCases",
                column: "RefCaseLevelId",
                principalTable: "RefCaseLevels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCases_RefCaseLevels_RefCaseLevelId",
                table: "TblCases");

            migrationBuilder.DropIndex(
                name: "IX_TblCases_RefCaseLevelId",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "RefCaseLevelId",
                table: "TblCases");

            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "TblCases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_LevelId",
                table: "TblCases",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCases_RefCaseLevels_LevelId",
                table: "TblCases",
                column: "LevelId",
                principalTable: "RefCaseLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
