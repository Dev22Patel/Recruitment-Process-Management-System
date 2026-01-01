using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User> CreateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<List<User>> GetUsersByRoleAsync(string roleName);
    }
}
