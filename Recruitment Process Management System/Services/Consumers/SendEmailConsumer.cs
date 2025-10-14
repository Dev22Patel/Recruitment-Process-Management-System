using MassTransit;
using Recruitment_Process_Management_System.Models.Events;

namespace Recruitment_Process_Management_System.Services.Consumers
{
    public class SendEmailConsumer : IConsumer<SendEmailEvent>
    {
        private readonly EmailService _emailService;
        private readonly ILogger<SendEmailConsumer> _logger;

        public SendEmailConsumer(EmailService emailService, ILogger<SendEmailConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendEmailEvent> context)
        {
            var emailEvent = context.Message;

            try
            {
                _logger.LogInformation($"Processing email for {emailEvent.ToEmail}");
                await _emailService.SendEmailAsync(
                    emailEvent.ToEmail,
                    emailEvent.Subject,
                    emailEvent.Body
                );
                _logger.LogInformation($"Successfully processed email for {emailEvent.ToEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing email to {emailEvent.ToEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
