using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,HR,Recruiter")]
    public class JobPositionReviewerController : ControllerBase
    {
        private readonly JobPositionReviewerService _jobPositionReviewerService;

        public JobPositionReviewerController(JobPositionReviewerService jobPositionReviewerService)
        {
            _jobPositionReviewerService = jobPositionReviewerService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // POST: api/JobPositionReviewer/assign
        [HttpPost("assign")]
        public async Task<IActionResult> AssignReviewer([FromBody] AssignReviewerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _jobPositionReviewerService.AssignReviewerToJobPositionAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // POST: api/JobPositionReviewer/bulk-assign
        [HttpPost("bulk-assign")]
        public async Task<IActionResult> BulkAssignReviewers([FromBody] BulkAssignReviewersDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _jobPositionReviewerService.BulkAssignReviewersAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // DELETE: api/JobPositionReviewer/remove/{id}
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveReviewer(Guid id)
        {
            var result = await _jobPositionReviewerService.RemoveReviewerFromJobPositionAsync(id);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // GET: api/JobPositionReviewer/job/{jobPositionId}
        [HttpGet("job/{jobPositionId}")]
        public async Task<IActionResult> GetReviewersForJob(Guid jobPositionId)
        {
            var reviewers = await _jobPositionReviewerService.GetReviewersForJobPositionAsync(jobPositionId);
            return Ok(reviewers);
        }

        // GET: api/JobPositionReviewer/my-assignments
        [HttpGet("my-assignments")]
        public async Task<IActionResult> GetMyAssignedJobs()
        {
            var userId = GetCurrentUserId();
            var assignments = await _jobPositionReviewerService.GetMyAssignedJobPositionsAsync(userId);
            return Ok(assignments);
        }
    }
}