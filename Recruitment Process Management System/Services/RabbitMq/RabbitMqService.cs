using MassTransit;
using Recruitment_Process_Management_System.Models.Events;

namespace Recruitment_Process_Management_System.Services.RabbitMq
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IPublishEndpoint publishEndpoint, ILogger<RabbitMqService> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishEmailAsync(SendEmailEvent emailEvent)
        {
            try
            {
                emailEvent.Id = Guid.NewGuid();
                emailEvent.CreatedAt = DateTime.UtcNow;

                await _publishEndpoint.Publish(emailEvent);
                _logger.LogInformation($"Email event published for {emailEvent.ToEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to publish email event: {ex.Message}");
                throw;
            }
        }
    }
}
