using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefCaseLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefCaseLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefCaseStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefCaseStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefCaseTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefCaseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefCourts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefCourts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefReliefSoughts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefReliefSoughts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefUserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefUserTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblCases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Claimant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Defendant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    CourtId = table.Column<int>(type: "int", nullable: false),
                    ClaimValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReliefSoughtId = table.Column<int>(type: "int", nullable: false),
                    ExpertWitnessId = table.Column<long>(type: "bigint", nullable: true),
                    ExpertWitnessNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpertWitnessNameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCases_RefCaseLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "RefCaseLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCases_RefCaseStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "RefCaseStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCases_RefCaseTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "RefCaseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCases_RefCourts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "RefCourts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCases_RefReliefSoughts_ReliefSoughtId",
                        column: x => x.ReliefSoughtId,
                        principalTable: "RefReliefSoughts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TblUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblUsers_RefUserTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "RefUserTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TblCaseTeam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCaseTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCaseTeam_TblCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "TblCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCaseTeam_TblUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TblUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_CourtId",
                table: "TblCases",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_LevelId",
                table: "TblCases",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_ReliefSoughtId",
                table: "TblCases",
                column: "ReliefSoughtId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_StatusId",
                table: "TblCases",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCases_TypeId",
                table: "TblCases",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTeam_CaseId",
                table: "TblCaseTeam",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseTeam_UserId",
                table: "TblCaseTeam",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TblUsers_TypeId",
                table: "TblUsers",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblCaseTeam");

            migrationBuilder.DropTable(
                name: "TblCases");

            migrationBuilder.DropTable(
                name: "TblUsers");

            migrationBuilder.DropTable(
                name: "RefCaseLevels");

            migrationBuilder.DropTable(
                name: "RefCaseStatus");

            migrationBuilder.DropTable(
                name: "RefCaseTypes");

            migrationBuilder.DropTable(
                name: "RefCourts");

            migrationBuilder.DropTable(
                name: "RefReliefSoughts");

            migrationBuilder.DropTable(
                name: "RefUserTypes");
        }
    }
}
