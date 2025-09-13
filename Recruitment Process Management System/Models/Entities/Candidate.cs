using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class Candidate
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // FIXED: Changed from int to Guid

        [MaxLength(100)]
        public string? CurrentLocation { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal? TotalExperience { get; set; }

        [MaxLength(200)]
        public string? CurrentCompany { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? CurrentSalary { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ExpectedSalary { get; set; }

        public int? NoticePeriod { get; set; }

        [MaxLength(50)]
        public string? Source { get; set; } = "Manual Entry";

        [MaxLength(200)]
        public string? CollegeName { get; set; }

        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? Degree { get; set; }

        [MaxLength(500)]
        public string? ResumeFilePath { get; set; }

        public bool IsProfileCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
