using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class JobPosition
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; } // Full-time, Part-time, Contract

        [MaxLength(50)]
        public string? ExperienceLevel { get; set; } // Entry, Mid, Senior

        public int? MinExperience { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? Salary { get; set; }

        [Required]
        public int StatusId { get; set; }

        [MaxLength(500)]
        public string? StatusReason { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User? Creator { get; set; }
        public virtual Status? Status { get; set; }
        public virtual ICollection<JobSkillRequirement> JobSkillRequirements { get; set; } = new List<JobSkillRequirement>();
    }
}
