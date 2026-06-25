using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class update_document_classification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseDocuments_RefDocumentClassifications_ClassificationId",
                table: "TblCaseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_TblCaseDocuments_ClassificationId",
                table: "TblCaseDocuments");

            migrationBuilder.DropColumn(
                name: "ClassificationId",
                table: "TblCaseDocuments");

            migrationBuilder.AddColumn<int>(
                name: "RefDocumentClassificationId",
                table: "TblCaseDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseDocuments_RefDocumentClassificationId",
                table: "TblCaseDocuments",
                column: "RefDocumentClassificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseDocuments_RefDocumentClassifications_RefDocumentClassificationId",
                table: "TblCaseDocuments",
                column: "RefDocumentClassificationId",
                principalTable: "RefDocumentClassifications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblCaseDocuments_RefDocumentClassifications_RefDocumentClassificationId",
                table: "TblCaseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_TblCaseDocuments_RefDocumentClassificationId",
                table: "TblCaseDocuments");

            migrationBuilder.DropColumn(
                name: "RefDocumentClassificationId",
                table: "TblCaseDocuments");

            migrationBuilder.AddColumn<int>(
                name: "ClassificationId",
                table: "TblCaseDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TblCaseDocuments_ClassificationId",
                table: "TblCaseDocuments",
                column: "ClassificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCaseDocuments_RefDocumentClassifications_ClassificationId",
                table: "TblCaseDocuments",
                column: "ClassificationId",
                principalTable: "RefDocumentClassifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
