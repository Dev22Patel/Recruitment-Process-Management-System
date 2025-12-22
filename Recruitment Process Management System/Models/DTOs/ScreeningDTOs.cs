using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs
{
    // Create Screening Review
    public class CreateScreeningReviewDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Range(1, 5)]
        public decimal? Rating { get; set; }

        [StringLength(2000)]
        public string? Comments { get; set; }

        [Required]
        public bool IsRecommendedForInterview { get; set; }

        public List<SkillVerificationDto>? VerifiedSkills { get; set; }
    }

    // Update Screening Review
    public class UpdateScreeningReviewDto
    {
        [Required]
        public Guid ScreeningReviewId { get; set; }

        [Range(1, 5)]
        public decimal? Rating { get; set; }

        [StringLength(2000)]
        public string? Comments { get; set; }

        [Required]
        public bool IsRecommendedForInterview { get; set; }

        public List<SkillVerificationDto>? VerifiedSkills { get; set; }
    }

    // Skill Verification DTO
    public class SkillVerificationDto
    {
        [Required]
        public Guid CandidateSkillId { get; set; }

        public string? SkillName { get; set; }

        public decimal? ClaimedYears { get; set; }

        [Range(0, 50)]
        public decimal? VerifiedYears { get; set; }

        [Required]
        public bool IsVerified { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }
    }

    // Screening Review Response
    public class ScreeningReviewResponseDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? JobTitle { get; set; }
        public Guid ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime ReviewDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public decimal? Rating { get; set; }
        public string? Comments { get; set; }
        public bool IsRecommendedForInterview { get; set; }
        public List<SkillVerificationDto>? VerifiedSkills { get; set; }
        public PreviousScreeningDto? PreviousScreening { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Previous Screening Info
    public class PreviousScreeningDto
    {
        public bool HasBeenScreenedBefore { get; set; }
        public List<ScreeningHistoryItemDto>? ScreeningHistory { get; set; }
    }

    // Screening History Item
    public class ScreeningHistoryItemDto
    {
        public Guid ScreeningReviewId { get; set; }
        public string? JobTitle { get; set; }
        public DateTime ScreenedDate { get; set; }
        public string? Result { get; set; }
        public string? ReviewerName { get; set; }
        public decimal? Rating { get; set; }
        public string? Comments { get; set; }
    }

    // Assign Reviewer DTO
    public class AssignReviewerDto
    {
        [Required]
        public Guid JobPositionId { get; set; }

        [Required]
        public Guid ReviewerId { get; set; }
    }

    // Bulk Assign Reviewers DTO
    public class BulkAssignReviewersDto
    {
        [Required]
        public Guid JobPositionId { get; set; }

        [Required]
        [MinLength(1)]
        public List<Guid> ReviewerIds { get; set; } = new();
    }

    // Job Position Reviewer Response
    public class JobPositionReviewerResponseDto
    {
        public Guid Id { get; set; }
        public Guid JobPositionId { get; set; }
        public string? JobTitle { get; set; }
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerEmail { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedByName { get; set; }
        public bool IsActive { get; set; }
    }

    // Pending Screening Response
    public class PendingScreeningResponseDto
    {
        public Guid ApplicationId { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? JobTitle { get; set; }
        public DateTime ApplicationDate { get; set; }
        public decimal? TotalExperience { get; set; }
        public string? CurrentCompany { get; set; }
        public string? ResumeFilePath { get; set; }
        public int MatchingSkills { get; set; }
        public int RequiredSkills { get; set; }
        public bool HasBeenScreenedBefore { get; set; }
        public List<CandidateSkillDto>? CandidateSkills { get; set; }
    }

    // Candidate Skill DTO
    public class CandidateSkillDto
    {
        public Guid CandidateSkillId { get; set; }
        public Guid SkillId { get; set; }
        public string? SkillName { get; set; }
        public decimal? YearsOfExperience { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRequired { get; set; }
    }

    // Screening Statistics
    public class ScreeningStatisticsDto
    {
        public int TotalPendingScreenings { get; set; }
        public int TotalCompletedScreenings { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal AverageRating { get; set; }
        public decimal ApprovalRate { get; set; }
        public List<ReviewerStatDto>? ReviewerStats { get; set; }
    }

    // Reviewer Statistics
    public class ReviewerStatDto
    {
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public int TotalReviews { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal AverageRating { get; set; }
        public decimal AvgReviewTime { get; set; } // in hours
    }
}