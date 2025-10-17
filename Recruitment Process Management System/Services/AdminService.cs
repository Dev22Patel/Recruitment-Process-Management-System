using Recruitment_Process_Management_System.Models.DTOs.Admin_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class AdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly ILogger<AdminService> _logger;
        private readonly EmailService _emailService;

        public AdminService(
            IAdminRepository adminRepository,
            ILogger<AdminService> logger,
            EmailService emailService)
        {
            _adminRepository = adminRepository;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<List<AdminEmployeeDto>> GetAllEmployeesAsync()
        {
            try
            {
                var employees = await _adminRepository.GetAllEmployeesAsync();

                return employees.Select(e => new AdminEmployeeDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    IsActive = e.IsActive,
                    Roles = e.UserRoles.Select(ur => new EmployeeRoleDto
                    {
                        Id = ur.Role.Id,
                        RoleName = ur.Role.RoleName
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all employees");
                throw;
            }
        }

        public async Task<AdminEmployeeDto?> CreateEmployeeAsync(CreateEmployeeRequestDto request, Guid createdBy)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _adminRepository.GetEmployeeByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("Email already exists");
                }

                // Generate random password
                var randomPassword = GenerateRandomPassword();

                // Create user entity - FIX: Set UserType to "Employee"
                var employee = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email.ToLower().Trim(),
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword),
                    UserType = "Employee", // CRITICAL FIX: Set UserType to Employee
                    IsActive = true,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                var createdEmployee = await _adminRepository.CreateEmployeeAsync(employee, request.RoleIds);

                // Send email with credentials
                try
                {
                    var roleNames = string.Join(", ", createdEmployee.UserRoles.Select(ur => ur.Role.RoleName));
                    var emailBody = $@"
                                    Hello {createdEmployee.FirstName} {createdEmployee.LastName},

                                    Your employee account has been created successfully.

                                    Login Credentials:
                                    Email: {createdEmployee.Email}
                                    Temporary Password: {randomPassword}

                                    Assigned Roles: {roleNames}

                                    Please login and change your password immediately for security purposes.

                                    Best regards,
                                    Recruitment Team";


                    await _emailService.QueueEmailAsync(
                        createdEmployee.Email,
                        "Employee Account Created - Login Credentials",
                        emailBody
                    );

                    _logger.LogInformation($"Employee created with email: {createdEmployee.Email}, credentials sent via email");
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"Failed to send email to {createdEmployee.Email}, but employee was created");
                    // Don't throw - employee is created, just email failed
                }

                return new AdminEmployeeDto
                {
                    Id = createdEmployee.Id,
                    FirstName = createdEmployee.FirstName,
                    LastName = createdEmployee.LastName,
                    Email = createdEmployee.Email,
                    PhoneNumber = createdEmployee.PhoneNumber,
                    IsActive = createdEmployee.IsActive,
                    Roles = createdEmployee.UserRoles.Select(ur => new EmployeeRoleDto
                    {
                        Id = ur.Role.Id,
                        RoleName = ur.Role.RoleName
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeStatusAsync(Guid employeeId, bool isActive)
        {
            try
            {
                return await _adminRepository.UpdateEmployeeStatusAsync(employeeId, isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee status for ID: {employeeId}");
                throw;
            }
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            try
            {
                return await _adminRepository.GetAdminStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin stats");
                throw;
            }
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                return await _adminRepository.GetAllRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                throw;
            }
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}