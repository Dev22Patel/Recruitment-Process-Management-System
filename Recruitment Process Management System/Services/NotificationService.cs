using Newtonsoft.Json;
using Recruitment_Process_Management_System.Models.DTOs.Notification_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using System.Text;
using RabbitMQ.Client;

namespace Recruitment_Process_Management_System.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IConnectionFactory _rabbitMqFactory;

        public NotificationService(INotificationRepository notificationRepository, IConnectionFactory rabbitMqFactory)
        {
            _notificationRepository = notificationRepository;
            _rabbitMqFactory = rabbitMqFactory; 
        }

        public async Task SendNotificationAsync(NotificationDto notificationDto)
        {
            // Insert into DB
            var notification = new Notification
            {
                UserId = notificationDto.UserId,
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                RelatedEntityType = notificationDto.RelatedEntityType,
                RelatedEntityId = notificationDto.RelatedEntityId,
                IsSent = false
            };

            var savedNotification = await _notificationRepository.AddNotificationAsync(notification);

            // Publish to RabbitMQ queue
            using var connection = await _rabbitMqFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var message = JsonConvert.SerializeObject(new { NotificationId = savedNotification.Id });
            var body = Encoding.UTF8.GetBytes(message);


            await channel.BasicPublishAsync(exchange: "", routingKey: "email_queue", body: body);
        }
    }
}
