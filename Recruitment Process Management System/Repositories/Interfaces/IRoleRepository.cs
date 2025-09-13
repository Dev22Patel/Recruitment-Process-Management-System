using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string roleName);
        Task<Role> CreateAsync(Role role);
        Task<List<string>> GetAllActiveRolesAsync();


    }
}
