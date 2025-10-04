using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Recruitment_Process_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPositionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExperienceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinExperience = table.Column<int>(type: "int", nullable: true),
                    MaxExperience = table.Column<int>(type: "int", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPositions_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPositions_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobSkillRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MinYearsExperience = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkillRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSkillRequirements_JobPositions_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "JobPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobSkillRequirements_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "IsActive", "RoleName" },
                values: new object[] { 5, true, "Recruiter" });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "Id", "EntityType", "IsActive", "StatusName" },
                values: new object[,]
                {
                    { 1, "JobPosition", true, "Open" },
                    { 2, "JobPosition", true, "On Hold" },
                    { 3, "JobPosition", true, "Closed" },
                    { 4, "Application", true, "Applied" },
                    { 5, "Application", true, "Screening" },
                    { 6, "Application", true, "Interview" },
                    { 7, "Application", true, "Selected" },
                    { 8, "Application", true, "Rejected" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_CreatorId",
                table: "JobPositions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_StatusId",
                table: "JobPositions",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkillRequirements_JobPositionId",
                table: "JobSkillRequirements",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkillRequirements_SkillId",
                table: "JobSkillRequirements",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSkillRequirements");

            migrationBuilder.DropTable(
                name: "JobPositions");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
