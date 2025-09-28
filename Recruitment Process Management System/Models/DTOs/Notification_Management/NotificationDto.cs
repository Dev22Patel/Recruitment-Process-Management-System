namespace Recruitment_Process_Management_System.Models.DTOs.Notification_Management
{
    public class NotificationDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
    }
}
