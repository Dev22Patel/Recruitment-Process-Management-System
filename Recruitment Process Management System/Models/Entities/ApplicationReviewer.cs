namespace Recruitment_Process_Management_System.Models.Entities
{
    public class ApplicationReviewer
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid ReviewerId { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Application? Application { get; set; }
        public User? Reviewer { get; set; }
    }
}