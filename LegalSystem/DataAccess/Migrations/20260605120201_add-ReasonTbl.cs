using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addReasonTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReasonId",
                table: "TblCaseTasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reasons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTasks_ReasonId",
                table: "TblCaseTasks",
                column: "ReasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseTasks_Reasons_ReasonId",
                table: "TblCaseTasks",
                column: "ReasonId",
                principalTable: "Reasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseTasks_Reasons_ReasonId",
                table: "TblCaseTasks");

            migrationBuilder.DropTable(
                name: "Reasons");

            migrationBuilder.DropIndex(
                name: "IX_TblCaseTasks_ReasonId",
                table: "TblCaseTasks");

            migrationBuilder.DropColumn(
                name: "ReasonId",
                table: "TblCaseTasks");
        }
    }
}
