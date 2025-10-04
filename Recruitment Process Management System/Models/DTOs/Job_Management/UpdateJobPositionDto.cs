using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class UpdateJobPositionDto
    {
        [Required]
        public Guid Id { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? EmploymentType { get; set; }

        [StringLength(50)]
        public string? ExperienceLevel { get; set; }

        public int? MinExperience { get; set; }
        public decimal? Salary { get; set; }
        public int? StatusId { get; set; }
        public string? StatusReason { get; set; }

        public List<JobSkillRequirementDto>? RequiredSkills { get; set; }
        public List<JobSkillRequirementDto>? PreferredSkills { get; set; }
        //public List<Guid>? ReviewerIds { get; set; }
    }
}
