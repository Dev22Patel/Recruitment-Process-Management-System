using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class ReviewerSkillVerification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ScreeningReviewId { get; set; }

        [Required]
        public Guid CandidateSkillId { get; set; }

        public bool IsVerified { get; set; }

        [Range(0, 50)]
        public decimal? VerifiedYearsOfExperience { get; set; }

        [Required]
        public Guid VerifiedBy { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ScreeningReviewId")]
        public virtual ScreeningReview? ScreeningReview { get; set; }

        [ForeignKey("CandidateSkillId")]
        public virtual CandidateSkill? CandidateSkill { get; set; }

        [ForeignKey("VerifiedBy")]
        public virtual User? Verifier { get; set; }
    }
}