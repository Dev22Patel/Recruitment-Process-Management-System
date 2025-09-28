using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddNotificationAsync(Notification notification);
        Task<Notification> GetNotificationByIdAsync(int id);
        Task UpdateNotificationAsync(Notification notification);
    }   
}
