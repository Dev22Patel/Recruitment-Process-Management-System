using Recruitment_Process_Management_System.Models.DTOs.Notification_Management;

namespace Recruitment_Process_Management_System.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationDto notificationDto);
    }
}
