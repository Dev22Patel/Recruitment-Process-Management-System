using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Services;
using System;
using System.Threading.Tasks;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActiveRoles()
        {
            try
            {
                var roles = await _roleService.GetAllActiveRolesAsync();
                return Ok(new { data = roles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching roles", error = ex.Message });
            }
        }
    }
}