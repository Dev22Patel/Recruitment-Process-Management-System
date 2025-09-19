using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class CandidateSkill
    {
        public Guid Id { get; set; } // Primary Key

        [Required]
        public Guid CandidateId { get; set; } // Foreign Key to Candidate

        [Required]
        public Guid SkillId { get; set; } // Foreign Key to Skill

        [Column(TypeName = "decimal(3,1)")]
        public decimal? YearsOfExperience { get; set; } // e.g., 2.5 years

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("CandidateId")]
        public Candidate Candidate { get; set; }

        [ForeignKey("SkillId")]
        public Skill Skill { get; set; }

        
    }
}
