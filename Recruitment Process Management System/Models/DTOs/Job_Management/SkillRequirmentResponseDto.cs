namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class SkillRequirementResponseDto
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; }
        public string? Category { get; set; }
        public int MinYearsExperience { get; set; }
    }
}
