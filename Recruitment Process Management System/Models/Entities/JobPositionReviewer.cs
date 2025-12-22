using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class JobPositionReviewer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid JobPositionId { get; set; }

        [Required]
        public Guid ReviewerId { get; set; }

        [Required]
        public Guid AssignedBy { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? RemovedAt { get; set; }

        // Navigation properties
        [ForeignKey("JobPositionId")]
        public virtual JobPosition? JobPosition { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual User? Reviewer { get; set; }

        [ForeignKey("AssignedBy")]
        public virtual User? Assigner { get; set; }
    }
}