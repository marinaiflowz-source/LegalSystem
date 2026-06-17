using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class update_CaseTbl_AddUnitInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitNumber",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectCode",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "UnitNumber",
                table: "TblCases");
        }
    }
}
