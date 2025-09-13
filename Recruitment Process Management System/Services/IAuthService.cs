using Recruitment_Process_Management_System.Models.DTOs;

namespace Recruitment_Process_Management_System.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
