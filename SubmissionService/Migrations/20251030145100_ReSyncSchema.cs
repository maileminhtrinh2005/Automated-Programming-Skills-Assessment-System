using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubmissionService.Migrations
{
    /// <inheritdoc />
    public partial class ReSyncSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    languageId = table.Column<int>(type: "int", nullable: false),
                    languageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Language__12696A62150DDB19", x => x.languageId);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Submissi__449EE1258400B2E4", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_Submissions_Languages",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "languageId");
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    TestCaseId = table.Column<int>(type: "int", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    ExecutionTime = table.Column<double>(type: "float", nullable: false),
                    MemoryUsed = table.Column<int>(type: "int", nullable: false),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Results__976902083B2A0C15", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_SubmissionResults_Submissions",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Results_SubmissionId",
                table: "Results",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_LanguageId",
                table: "Submissions",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
