namespace Recruitment_Process_Management_System.Models.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime? ReadAt { get; set; }
        public string RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Additional field for email status (extend the table if needed, but for simplicity, add here)
        public bool IsSent { get; set; } = false; // Track if email was sent
    }
}
