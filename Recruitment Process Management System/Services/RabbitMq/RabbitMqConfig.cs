namespace Recruitment_Process_Management_System.Services.RabbitMq
{
    public class RabbitMqConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
    }
}
