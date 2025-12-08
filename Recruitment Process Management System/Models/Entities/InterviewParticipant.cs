using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    [Table("InterviewParticipants")]
    public class InterviewParticipant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InterviewRoundId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ParticipantType { get; set; } // Primary_Interviewer, Co_Interviewer

        [Required]
        public int AttendanceStatusId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("InterviewRoundId")]
        public virtual InterviewRound? InterviewRound { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("AttendanceStatusId")]
        public virtual Status? AttendanceStatus { get; set; }
    }
}