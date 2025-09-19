using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class Skill
    {
        public Guid Id { get; set; }   // Primary Key

        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; } = string.Empty;  // e.g. "React.js"

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;   // e.g. "Frontend", "Backend"

        public bool IsActive { get; set; } = true;             // For soft delete / inactive skills

        // Relationships
        public ICollection<CandidateSkill> CandidateSkills { get; set; } // Many-to-many with Candidates
    }
}
