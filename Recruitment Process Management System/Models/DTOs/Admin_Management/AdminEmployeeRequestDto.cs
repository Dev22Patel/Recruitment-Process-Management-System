using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Admin_Management
{
    public class CreateEmployeeRequestDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "At least one role must be selected")]
        [MinLength(1, ErrorMessage = "At least one role must be selected")]
        public List<int> RoleIds { get; set; } = new();
    }

    public class UpdateEmployeeStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }

    public class AdminStatsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveRecruiters { get; set; }
        public int ActiveInterviewers { get; set; }
    }
}
