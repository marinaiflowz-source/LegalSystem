using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Modify_TblCaseAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionTypeId",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "AffectedColumns",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "ChildId",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "ChildTypeId",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "UpdatedByName",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "TblCaseAuditLog");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "TblCaseAuditLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionTypeId",
                table: "TblCaseAuditLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AffectedColumns",
                table: "TblCaseAuditLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChildId",
                table: "TblCaseAuditLog",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ChildTypeId",
                table: "TblCaseAuditLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedById",
                table: "TblCaseAuditLog",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByName",
                table: "TblCaseAuditLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "TblCaseAuditLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "TblCaseAuditLog",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
