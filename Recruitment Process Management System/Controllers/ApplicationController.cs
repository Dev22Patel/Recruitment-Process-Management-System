using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs.Application_Management;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationController : ControllerBase
    {
        private readonly ApplicationService _applicationService;

        public ApplicationController(ApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // POST: api/Application/apply
        [HttpPost("apply")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> ApplyForJob([FromBody] CreateApplicationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var result = await _applicationService.CreateApplicationAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                application = result.Application
            });
        }

        // GET: api/Application/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicationById(Guid id)
        {
            var application = await _applicationService.GetApplicationByIdAsync(id);

            if (application == null)
            {
                return NotFound(new { message = "Application not found" });
            }

            return Ok(application);
        }

        // GET: api/Application/my-applications
        [HttpGet("my-applications")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var applications = await _applicationService.GetCandidateApplicationsAsync(userId);
            return Ok(applications);
        }

        // GET: api/Application/job/{jobPositionId}
        [HttpGet("job/{jobPositionId}")]
        [Authorize(Roles = "Admin,HR,Recruiter")]
        public async Task<IActionResult> GetApplicationsByJob(Guid jobPositionId)
        {
            var applications = await _applicationService.GetApplicationsByJobPositionAsync(jobPositionId);
            return Ok(applications);
        }

        // GET: api/Application/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR,Recruiter")]
        public async Task<IActionResult> GetAllApplications()
        {
            var applications = await _applicationService.GetAllApplicationsAsync();
            return Ok(applications);
        }

        // PUT: api/Application/update-status
        [HttpPut("update-status")]
        [Authorize(Roles = "Admin,HR,Recruiter")]
        public async Task<IActionResult> UpdateApplicationStatus([FromBody] UpdateApplicationStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _applicationService.UpdateApplicationStatusAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // GET: api/Application/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,HR,Recruiter")]
        public async Task<IActionResult> GetApplicationStatistics([FromQuery] Guid? jobPositionId = null)
        {
            var statistics = await _applicationService.GetApplicationStatisticsAsync(jobPositionId);
            return Ok(statistics);
        }

        // DELETE: api/Application/withdraw/{applicationId}
        [HttpDelete("withdraw/{applicationId}")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> WithdrawApplication(Guid applicationId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var result = await _applicationService.WithdrawApplicationAsync(applicationId, userId);

            if (!result)
            {
                return BadRequest(new { message = "Unable to withdraw application. It may not exist or is already processed." });
            }

            return Ok(new { message = "Application withdrawn successfully" });
        }

        // Helper method to get current user ID from JWT token
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}