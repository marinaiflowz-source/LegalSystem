using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaseTbl_InventoryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerNumber",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JointBuyerMobile",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JointBuyerName",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadStatus",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoldPrice",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitType",
                table: "TblCases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "BuyerNumber",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "JointBuyerMobile",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "JointBuyerName",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "LeadStatus",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "SoldPrice",
                table: "TblCases");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "TblCases");
        }
    }
}
