using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class CreateJobPositionDto
    {
        [Required(ErrorMessage = "Job title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? EmploymentType { get; set; } // Full-time, Part-time, Contract

        [StringLength(50)]
        public string? ExperienceLevel { get; set; } // Entry, Mid, Senior

        [Range(0, 50, ErrorMessage = "Minimum experience must be between 0 and 50")]
        public int? MinExperience { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive")]
        public decimal? Salary { get; set; }

        public int StatusId { get; set; } = 1; // Default to "Open" or similar

        public string? StatusReason { get; set; }

        // Required and preferred skills
        public List<JobSkillRequirementDto> RequiredSkills { get; set; } = new();
        public List<JobSkillRequirementDto> PreferredSkills { get; set; } = new();

        //// Optional: Assign reviewers during creation
        //public List<Guid> ReviewerIds { get; set; } = new();
    }
}
