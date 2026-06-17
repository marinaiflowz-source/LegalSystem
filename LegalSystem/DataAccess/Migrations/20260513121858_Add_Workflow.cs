using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TblCases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TblWorkFlows",
                columns: table => new
                {
                    CaseTypeId = table.Column<int>(type: "int", nullable: false),
                    CaseStatusId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblWorkFlows", x => new { x.CaseTypeId, x.CaseStatusId });
                    table.ForeignKey(
                        name: "FK_TblWorkFlows_RefCaseStatus_CaseStatusId",
                        column: x => x.CaseStatusId,
                        principalTable: "RefCaseStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblWorkFlows_RefCaseTypes_CaseTypeId",
                        column: x => x.CaseTypeId,
                        principalTable: "RefCaseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblWorkFlows_CaseStatusId",
                table: "TblWorkFlows",
                column: "CaseStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblWorkFlows");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TblCases");
        }
    }
}
