using Recruitment_Process_Management_System.Models.DTOs.Admin_Management;
using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<List<User>> GetAllEmployeesAsync();
        Task<User?> GetEmployeeByIdAsync(Guid employeeId);
        Task<User?> GetEmployeeByEmailAsync(string email);
        Task<User> CreateEmployeeAsync(User employee, List<int> roleIds);
        Task<bool> UpdateEmployeeStatusAsync(Guid employeeId, bool isActive);
        Task<AdminStatsDto> GetAdminStatsAsync();
        Task<List<Role>> GetAllRolesAsync();
    }
}
