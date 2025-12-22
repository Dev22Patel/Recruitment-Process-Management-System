using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class ScreeningReview
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public Guid ReviewedBy { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public int StatusId { get; set; }

        [Range(1, 5)]
        public decimal? Rating { get; set; }

        [StringLength(2000)]
        public string? Comments { get; set; }

        public bool IsRecommendedForInterview { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public virtual Application? Application { get; set; }

        [ForeignKey("ReviewedBy")]
        public virtual User? Reviewer { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status? Status { get; set; }

        public virtual ICollection<ReviewerSkillVerification>? SkillVerifications { get; set; }
    }
}