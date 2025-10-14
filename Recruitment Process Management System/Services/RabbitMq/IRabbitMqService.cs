using Recruitment_Process_Management_System.Models.Events;

namespace Recruitment_Process_Management_System.Services.RabbitMq
{
    public interface IRabbitMqService
    {
        Task PublishEmailAsync(SendEmailEvent emailEvent);
    }
}
