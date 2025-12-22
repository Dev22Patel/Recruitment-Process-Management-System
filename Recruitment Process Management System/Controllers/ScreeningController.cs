using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Reviewer,Hr,Admin")]
    public class ScreeningController : ControllerBase
    {
        private readonly ScreeningService _screeningService;

        public ScreeningController(ScreeningService screeningService)
        {
            _screeningService = screeningService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // POST: api/Screening/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateScreeningReview([FromBody] CreateScreeningReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _screeningService.CreateScreeningReviewAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                screeningReview = result.ScreeningReview
            });
        }

        // PUT: api/Screening/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateScreeningReview([FromBody] UpdateScreeningReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _screeningService.UpdateScreeningReviewAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // GET: api/Screening/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScreeningReviewById(Guid id)
        {
            var screeningReview = await _screeningService.GetScreeningReviewByIdAsync(id);

            if (screeningReview == null)
            {
                return NotFound(new { message = "Screening review not found" });
            }

            return Ok(screeningReview);
        }

        // GET: api/Screening/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingScreenings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var pendingScreenings = await _screeningService.GetPendingScreeningsAsync(userId);
                return Ok(pendingScreenings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Screening/application/{applicationId}
        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetScreeningsByApplication(Guid applicationId)
        {
            var screenings = await _screeningService.GetScreeningsByApplicationAsync(applicationId);
            return Ok(screenings);
        }



        // GET: api/Screening/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetScreeningStatistics([FromQuery] Guid? reviewerId = null)
        {
            try
            {
                var statistics = await _screeningService.GetScreeningStatisticsAsync(reviewerId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}