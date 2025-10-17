using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs.Admin_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Get all employees (excluding candidates)
        /// </summary>
        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _adminService.GetAllEmployeesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Employees retrieved successfully",
                    data = employees
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching employees"
                });
            }
        }

        /// <summary>
        /// Create a new employee
        /// </summary>
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var createdBy))
                {
                    return Unauthorized(new { success = false, message = "Invalid user session" });
                }

                var employee = await _adminService.CreateEmployeeAsync(request, createdBy);

                return Ok(new
                {
                    success = true,
                    message = "Employee created successfully. Login credentials sent to email.",
                    data = employee
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating employee"
                });
            }
        }

        /// <summary>
        /// Update employee status (activate/deactivate)
        /// </summary>
        [HttpPatch("employees/{employeeId}/status")]
        public async Task<IActionResult> UpdateEmployeeStatus(Guid employeeId, [FromBody] UpdateEmployeeStatusDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data"
                    });
                }

                var updated = await _adminService.UpdateEmployeeStatusAsync(employeeId, request.IsActive);

                if (!updated)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Employee not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Employee {(request.IsActive ? "activated" : "deactivated")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee status for ID: {employeeId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating employee status"
                });
            }
        }

        /// <summary>
        /// Get admin dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetAdminStats()
        {
            try
            {
                var stats = await _adminService.GetAdminStatsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Statistics retrieved successfully",
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin stats");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching statistics"
                });
            }
        }

        /// <summary>
        /// Get all available roles
        /// </summary>
        [HttpGet("role")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _adminService.GetAllRolesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Roles retrieved successfully",
                    data = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching roles"
                });
            }
        }
    }
}
