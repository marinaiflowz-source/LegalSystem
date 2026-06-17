using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_AuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedName",
                table: "TblUsers",
                newName: "UpdatedByName");

            migrationBuilder.RenameColumn(
                name: "UpdatedName",
                table: "TblCaseTeams",
                newName: "UpdatedByName");

            migrationBuilder.RenameColumn(
                name: "UpdatedName",
                table: "TblCaseTasks",
                newName: "UpdatedByName");

            migrationBuilder.RenameColumn(
                name: "UpdatedName",
                table: "TblCases",
                newName: "UpdatedByName");

            migrationBuilder.RenameColumn(
                name: "UpdatedName",
                table: "TblCaseEvent",
                newName: "UpdatedByName");

            migrationBuilder.RenameColumn(
                name: "UpdatedName",
                table: "TblCaseDocuments",
                newName: "UpdatedByName");

            migrationBuilder.CreateTable(
                name: "TblAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblAuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblAuditLogs");

            migrationBuilder.RenameColumn(
                name: "UpdatedByName",
                table: "TblUsers",
                newName: "UpdatedName");

            migrationBuilder.RenameColumn(
                name: "UpdatedByName",
                table: "TblCaseTeams",
                newName: "UpdatedName");

            migrationBuilder.RenameColumn(
                name: "UpdatedByName",
                table: "TblCaseTasks",
                newName: "UpdatedName");

            migrationBuilder.RenameColumn(
                name: "UpdatedByName",
                table: "TblCases",
                newName: "UpdatedName");

            migrationBuilder.RenameColumn(
                name: "UpdatedByName",
                table: "TblCaseEvent",
                newName: "UpdatedName");

            migrationBuilder.RenameColumn(
                name: "UpdatedByName",
                table: "TblCaseDocuments",
                newName: "UpdatedName");
        }
    }
}
