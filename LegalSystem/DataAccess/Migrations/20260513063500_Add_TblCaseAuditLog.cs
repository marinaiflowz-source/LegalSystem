using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_TblCaseAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TblCaseAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    ChildTypeId = table.Column<int>(type: "int", nullable: false),
                    ChildId = table.Column<long>(type: "bigint", nullable: false),
                    ActionTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCaseAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCaseAuditLog_TblCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "TblCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseAuditLog_CaseId",
                table: "TblCaseAuditLog",
                column: "CaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblCaseAuditLog");
        }
    }
}
