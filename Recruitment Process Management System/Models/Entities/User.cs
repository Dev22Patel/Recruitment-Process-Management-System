using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; } // Auto-generate GUID

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? PasswordHash { get; set; }

        [Required, MaxLength(20)]
        public string UserType { get; set; } = "Candidate"; // ADD THIS - was missing!

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; } // Changed to Guid to match User.Id

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
