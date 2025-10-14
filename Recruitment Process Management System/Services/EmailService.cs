using Recruitment_Process_Management_System.Models.Events;
using Recruitment_Process_Management_System.Services.RabbitMq;
using System.Net;
using System.Net.Mail;

namespace Recruitment_Process_Management_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IRabbitMqService rabbitMqService,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        // Called by AuthService - queues the job to RabbitMQ
        public async Task QueueEmailAsync(string toEmail, string subject, string body)
        {
            var emailEvent = new SendEmailEvent
            {
                ToEmail = toEmail,
                Subject = subject,
                Body = body
            };

            await _rabbitMqService.PublishEmailAsync(emailEvent);
        }

        // Called by SendEmailConsumer - actually sends the email
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"]),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                    Timeout = 10000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["Username"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
