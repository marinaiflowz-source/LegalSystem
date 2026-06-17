using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_TblCaseTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseTeam_TblCases_CaseId",
                table: "TblCaseTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseTeam_TblUsers_UserId",
                table: "TblCaseTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TblCaseTeam",
                table: "TblCaseTeam");

            migrationBuilder.RenameTable(
                name: "TblCaseTeam",
                newName: "TblCaseTeams");

            migrationBuilder.RenameIndex(
                name: "IX_TblCaseTeam_UserId",
                table: "TblCaseTeams",
                newName: "IX_TblCaseTeams_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TblCaseTeam_CaseId",
                table: "TblCaseTeams",
                newName: "IX_TblCaseTeams_CaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TblCaseTeams",
                table: "TblCaseTeams",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TblCaseTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    AssignedUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCaseTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCaseTasks_RefTaskPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "RefTaskPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCaseTasks_RefTaskStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "RefTaskStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCaseTasks_TblCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "TblCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCaseTasks_TblUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "TblUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTasks_AssignedUserId",
                table: "TblCaseTasks",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTasks_CaseId",
                table: "TblCaseTasks",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTasks_PriorityId",
                table: "TblCaseTasks",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTasks_StatusId",
                table: "TblCaseTasks",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseTeams_TblCases_CaseId",
                table: "TblCaseTeams",
                column: "CaseId",
                principalTable: "TblCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseTeams_TblUsers_UserId",
                table: "TblCaseTeams",
                column: "UserId",
                principalTable: "TblUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseTeams_TblCases_CaseId",
                table: "TblCaseTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseTeams_TblUsers_UserId",
                table: "TblCaseTeams");

            migrationBuilder.DropTable(
                name: "TblCaseTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TblCaseTeams",
                table: "TblCaseTeams");

            migrationBuilder.RenameTable(
                name: "TblCaseTeams",
                newName: "TblCaseTeam");

            migrationBuilder.RenameIndex(
                name: "IX_TblCaseTeams_UserId",
                table: "TblCaseTeam",
                newName: "IX_TblCaseTeam_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TblCaseTeams_CaseId",
                table: "TblCaseTeam",
                newName: "IX_TblCaseTeam_CaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TblCaseTeam",
                table: "TblCaseTeam",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseTeam_TblCases_CaseId",
                table: "TblCaseTeam",
                column: "CaseId",
                principalTable: "TblCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseTeam_TblUsers_UserId",
                table: "TblCaseTeam",
                column: "UserId",
                principalTable: "TblUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
