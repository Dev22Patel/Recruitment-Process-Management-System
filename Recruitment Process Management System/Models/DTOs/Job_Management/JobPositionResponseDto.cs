namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class JobPositionResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? ExperienceLevel { get; set; }
        public int? MinExperience { get; set; }
        public decimal? Salary { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? StatusReason { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<SkillRequirementResponseDto> RequiredSkills { get; set; } = new();
        public List<SkillRequirementResponseDto> PreferredSkills { get; set; } = new();
        //public List<ReviewerResponseDto> Reviewers { get; set; } = new();
    }
}
