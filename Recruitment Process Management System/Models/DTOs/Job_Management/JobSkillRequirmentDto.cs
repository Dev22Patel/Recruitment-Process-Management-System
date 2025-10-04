using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class JobSkillRequirementDto
    {
        [Required]
        public Guid SkillId { get; set; }

        [Range(0, 30, ErrorMessage = "Years of experience must be between 0 and 30")]
        public int MinYearsExperience { get; set; } = 0;
    }
}
