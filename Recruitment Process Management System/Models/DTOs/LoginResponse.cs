namespace Recruitment_Process_Management_System.Models.DTOs
{
    public class LoginResponse
    {
        public string UserId { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
