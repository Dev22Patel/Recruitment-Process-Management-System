namespace Recruitment_Process_Management_System.Models.Events
{
    public class SendEmailEvent
    {
        public Guid Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
