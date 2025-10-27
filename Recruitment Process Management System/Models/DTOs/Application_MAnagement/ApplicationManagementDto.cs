using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Application_Management
{
    // DTO for creating a new application
    public class CreateApplicationDto
    {
        [Required(ErrorMessage = "Job Position ID is required")]
        public Guid JobPositionId { get; set; }

        [MaxLength(500)]
        public string? CoverLetter { get; set; }
    }

    // DTO for application response
    public class ApplicationResponseDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public Guid JobPositionId { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public DateTime ApplicationDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? StatusReason { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional candidate info
        public decimal? TotalExperience { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public int? NoticePeriod { get; set; }
        public string? ResumeFilePath { get; set; }

        // Skills matching
        public List<string>? CandidateSkills { get; set; }
        public List<string>? RequiredSkills { get; set; }
        public int? MatchingSkillsCount { get; set; }
    }

    // DTO for listing applications (simplified view)
    public class ApplicationListDto
    {
        public Guid Id { get; set; }
        public Guid JobPositionId { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? StatusName { get; set; }
        public int? MatchingSkillsPercentage { get; set; }
    }

    // DTO for updating application status
    public class UpdateApplicationStatusDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public int StatusId { get; set; }

        [MaxLength(500)]
        public string? StatusReason { get; set; }
    }

    // DTO for application statistics
    public class ApplicationStatisticsDto
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int InScreeningApplications { get; set; }
        public int InInterviewApplications { get; set; }
        public int SelectedApplications { get; set; }
        public int RejectedApplications { get; set; }
    }

    // DTO for candidate's application history
    public class CandidateApplicationHistoryDto
    {
        public Guid ApplicationId { get; set; }
        public Guid JobPositionId { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? CurrentStatus { get; set; }
        public string? StatusReason { get; set; }
    }
}