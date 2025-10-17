namespace Recruitment_Process_Management_System.Models.DTOs.Admin_Management
{
    public class AdminEmployeeDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<EmployeeRoleDto> Roles { get; set; } = new();
    }

    public class EmployeeRoleDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
