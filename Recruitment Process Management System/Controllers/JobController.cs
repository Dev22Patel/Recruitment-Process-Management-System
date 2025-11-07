using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Candidate")]
    public class JobController : ControllerBase
    {
        private readonly JobService _jobService;

        public JobController(JobService jobService)
        {
            this._jobService = jobService;
        }

        // GET: api/Job/check-eligibility/{jobId}
        [HttpGet("check-eligibility/{jobId}")]
        public async Task<IActionResult> CheckEligibility(Guid jobId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid user token" });

            var result = await _jobService.CheckEligibilityAsync(userId, jobId);

            // Always return Ok with the eligibility result
            // Let the frontend handle the isEligible flag
            return Ok(new
            {
                isEligible = result.IsEligible,
                message = result.Message,
                skillMatchPercentage = result.SkillMatchPercentage,
                matchedRequiredSkills = result.MatchedRequiredSkills,
                totalRequiredSkills = result.TotalRequiredSkills,
                matchedPreferredSkills = result.MatchedPreferredSkills,
                totalPreferredSkills = result.TotalPreferredSkills
            });
        }

        // Helper method to get current user ID from JWT token
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}