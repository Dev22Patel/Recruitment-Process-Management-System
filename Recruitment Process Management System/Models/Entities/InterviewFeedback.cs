using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    [Table("InterviewFeedback")]
    public class InterviewFeedback
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InterviewRoundId { get; set; }

        [Required]
        public Guid InterviewerId { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? OverallRating { get; set; } // 1-5 rating

        [Column(TypeName = "decimal(3,2)")]
        public decimal? TechnicalRating { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? CommunicationRating { get; set; }

        public string? Comments { get; set; }

        public string? Recommendation { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("InterviewRoundId")]
        public virtual InterviewRound? InterviewRound { get; set; }

        [ForeignKey("InterviewerId")]
        public virtual User? Interviewer { get; set; }
    }
}