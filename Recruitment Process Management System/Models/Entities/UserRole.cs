namespace Recruitment_Process_Management_System.Models.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; } // FIXED: Changed from int to Guid
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
