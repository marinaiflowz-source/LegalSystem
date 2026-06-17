using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskCaseTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskTypeId",
                table: "TblCaseTasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTasks_TaskTypeId",
                table: "TblCaseTasks",
                column: "TaskTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseTasks_RefTaskTypes_TaskTypeId",
                table: "TblCaseTasks",
                column: "TaskTypeId",
                principalTable: "RefTaskTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseTasks_RefTaskTypes_TaskTypeId",
                table: "TblCaseTasks");

            migrationBuilder.DropIndex(
                name: "IX_TblCaseTasks_TaskTypeId",
                table: "TblCaseTasks");

            migrationBuilder.DropColumn(
                name: "TaskTypeId",
                table: "TblCaseTasks");
        }
    }
}
