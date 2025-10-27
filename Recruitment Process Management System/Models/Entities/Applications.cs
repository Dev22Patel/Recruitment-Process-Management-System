using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    [Table("Applications")]
    public class Application
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid JobPositionId { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int StatusId { get; set; }

        [MaxLength(500)]
        public string? StatusReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CandidateId")]
        public virtual Candidate? Candidate { get; set; }

        [ForeignKey("JobPositionId")]
        public virtual JobPosition? JobPosition { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status? Status { get; set; }

        // Related entities
        //public virtual ICollection<ScreeningReview>? ScreeningReviews { get; set; }
        //public virtual ICollection<InterviewRound>? InterviewRounds { get; set; }
        //public virtual ICollection<Offer>? Offers { get; set; }
    }
}