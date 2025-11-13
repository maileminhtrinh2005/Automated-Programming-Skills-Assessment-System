using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetailedFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    AssignmentTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailedFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", maxLength: 64, nullable: false),
                    AssignmentTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedFeedbacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManualFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssignmentTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualFeedbacks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailedFeedbacks");

            migrationBuilder.DropTable(
                name: "GeneratedFeedbacks");

            migrationBuilder.DropTable(
                name: "ManualFeedbacks");
        }
    }
}
