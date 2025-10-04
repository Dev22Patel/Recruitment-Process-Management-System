using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class JobSkillRequirement
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid JobPositionId { get; set; }

        [Required]
        public Guid SkillId { get; set; }

        public bool IsRequired { get; set; } = true; // Required vs Preferred

        public int MinYearsExperience { get; set; } = 0;

        // Navigation properties
        public virtual JobPosition? JobPosition { get; set; }
        public virtual Skill? Skill { get; set; }
    }
}
