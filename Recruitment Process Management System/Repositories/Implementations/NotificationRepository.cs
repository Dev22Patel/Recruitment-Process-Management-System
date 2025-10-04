using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            Console.WriteLine("Added notification to context.");
            Console.WriteLine($"Notification Details: UserId={notification.UserId}, Title={notification.Title}, Message={notification.Message}");
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }
}
