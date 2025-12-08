using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    [Table("InterviewRounds")]
    public class InterviewRound
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public int RoundNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoundType { get; set; } // Technical, HR, Panel

        [MaxLength(100)]
        public string? RoundName { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public int? Duration { get; set; } // Duration in minutes

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public int StatusId { get; set; }

        public Guid? CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public virtual Application? Application { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status? Status { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        // Related entities
        public virtual ICollection<InterviewParticipant> InterviewParticipants { get; set; } = new List<InterviewParticipant>();
        public virtual ICollection<InterviewFeedback> InterviewFeedbacks { get; set; } = new List<InterviewFeedback>();
    }
}