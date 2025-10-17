using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.DTOs.Admin_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllEmployeesAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName != "Candidate"))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetEmployeeByIdAsync(Guid employeeId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == employeeId);
        }

        public async Task<User?> GetEmployeeByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> CreateEmployeeAsync(User employee, List<int> roleIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Add user
                _context.Users.Add(employee);
                await _context.SaveChangesAsync();

                // Add user roles
                foreach (var roleId in roleIds)
                {
                    var userRole = new UserRole
                    {
                        UserId = employee.Id,
                        RoleId = roleId
                    };
                    _context.UserRoles.Add(userRole);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with roles
                return await GetEmployeeByIdAsync(employee.Id) ?? employee;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeStatusAsync(Guid employeeId, bool isActive)
        {
            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null)
                return false;

            employee.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            var totalEmployees = await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName != "Candidate"))
                .CountAsync();

            var activeRecruiters = await _context.Users
                .Where(u => u.IsActive && u.UserRoles.Any(ur => ur.Role.RoleName == "Recruiter"))
                .CountAsync();

            var activeInterviewers = await _context.Users
                .Where(u => u.IsActive && u.UserRoles.Any(ur => ur.Role.RoleName == "Interviewer"))
                .CountAsync();

            return new AdminStatsDto
            {
                TotalEmployees = totalEmployees,
                ActiveRecruiters = activeRecruiters,
                ActiveInterviewers = activeInterviewers
            };
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive && r.RoleName != "Candidate")
                .OrderBy(r => r.RoleName)
                .ToListAsync();
        }
    }
}
