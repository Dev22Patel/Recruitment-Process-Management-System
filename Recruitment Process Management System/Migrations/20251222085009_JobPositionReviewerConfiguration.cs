using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Recruitment_Process_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class JobPositionReviewerConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPositionReviewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPositionReviewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPositionReviewers_JobPositions_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "JobPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPositionReviewers_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPositionReviewers_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsRecommendedForInterview = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningReviews_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningReviews_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningReviews_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewerSkillVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScreeningReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedYearsOfExperience = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewerSkillVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewerSkillVerifications_CandidateSkills_CandidateSkillId",
                        column: x => x.CandidateSkillId,
                        principalTable: "CandidateSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewerSkillVerifications_ScreeningReviews_ScreeningReviewId",
                        column: x => x.ScreeningReviewId,
                        principalTable: "ScreeningReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewerSkillVerifications_Users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "Id", "EntityType", "IsActive", "StatusName" },
                values: new object[,]
                {
                    { 15, "Screening", true, "Pending Review" },
                    { 16, "Screening", true, "Approved" },
                    { 17, "Screening", true, "Rejected" },
                    { 18, "Screening", true, "Needs Clarification" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPositionReviewers_AssignedBy",
                table: "JobPositionReviewers",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositionReviewers_JobPositionId",
                table: "JobPositionReviewers",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositionReviewers_ReviewerId",
                table: "JobPositionReviewers",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerSkillVerifications_CandidateSkillId",
                table: "ReviewerSkillVerifications",
                column: "CandidateSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerSkillVerifications_ScreeningReviewId",
                table: "ReviewerSkillVerifications",
                column: "ScreeningReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerSkillVerifications_VerifiedBy",
                table: "ReviewerSkillVerifications",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningReviews_ApplicationId",
                table: "ScreeningReviews",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningReviews_ReviewedBy",
                table: "ScreeningReviews",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningReviews_StatusId",
                table: "ScreeningReviews",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPositionReviewers");

            migrationBuilder.DropTable(
                name: "ReviewerSkillVerifications");

            migrationBuilder.DropTable(
                name: "ScreeningReviews");

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 18);
        }
    }
}
