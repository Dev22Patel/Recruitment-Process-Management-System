using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs.Job_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles="Admin,HR")] // Only Admin and HR can manage job positions
    public class JobPositionsController : ControllerBase
    {
        private readonly JobPositionService _jobPositionService;

        public JobPositionsController(JobPositionService jobPositionService)
        {
            _jobPositionService = jobPositionService;
        }

        // POST: api/JobPositions
        [HttpPost]
        public async Task<ActionResult<JobPositionResponseDto>> CreateJobPosition([FromBody] CreateJobPositionDto dto)
        {
            try
            {
                

                // With this line to use a valid Guid:
                var userId = Guid.Parse("471a7d19-5eb0-484d-87b3-ca5464593daa");
                var result = await _jobPositionService.CreateJobPositionAsync(dto, userId);
                return CreatedAtAction(nameof(GetJobPositionById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the job position", error = ex.Message });
            }
        }

        // GET: api/JobPositions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<JobPositionResponseDto>> GetJobPositionById(Guid id)
        {
            try
            {
                var result = await _jobPositionService.GetJobPositionByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = "Job position not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the job position", error = ex.Message });
            }
        }

        // GET: api/JobPositions
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<JobPositionResponseDto>>> GetAllJobPositions()
        {
            try
            {
                var result = await _jobPositionService.GetAllJobPositionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching job positions", error = ex.Message });
            }
        }

        // GET: api/JobPositions/active
        [HttpGet("active")]
        [AllowAnonymous] // Allow candidates to view active job openings
        public async Task<ActionResult<List<JobListingDto>>> GetActiveJobListings()
        {
            try
            {
                var result = await _jobPositionService.GetActiveJobListingsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching active job listings", error = ex.Message });
            }
        }

        // PUT: api/JobPositions/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<JobPositionResponseDto>> UpdateJobPosition(Guid id, [FromBody] UpdateJobPositionDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = await _jobPositionService.UpdateJobPositionAsync(dto, userId);
                if (result == null)
                {
                    return NotFound(new { message = "Job position not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the job position", error = ex.Message });
            }
        }

        // DELETE: api/JobPositions/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJobPosition(Guid id)
        {
            try
            {
                var result = await _jobPositionService.DeleteJobPositionAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Job position not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the job position", error = ex.Message });
            }
        }
    }
}
