using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recruitment_Process_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateSkills_Users_VerifiedBy",
                table: "CandidateSkills");

            migrationBuilder.DropIndex(
                name: "IX_CandidateSkills_VerifiedBy",
                table: "CandidateSkills");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "CandidateSkills");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedBy",
                table: "CandidateSkills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_VerifiedBy",
                table: "CandidateSkills",
                column: "VerifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateSkills_Users_VerifiedBy",
                table: "CandidateSkills",
                column: "VerifiedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
