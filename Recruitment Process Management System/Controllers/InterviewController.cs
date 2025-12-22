using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Hr,Interviewer,Recruiter")]
    public class InterviewController : ControllerBase
    {
        private readonly InterviewService _interviewService;

        public InterviewController(InterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // POST: api/Interview/schedule
        [HttpPost("schedule")]
        [Authorize(Roles = "Admin,Hr,")]
        public async Task<IActionResult> ScheduleInterview([FromBody] CreateInterviewRoundDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _interviewService.CreateInterviewRoundAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                interviewRound = result.InterviewRound
            });
        }

        // GET: api/Interview/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInterviewRoundById(Guid id)
        {
            var interviewRound = await _interviewService.GetInterviewRoundByIdAsync(id);

            if (interviewRound == null)
            {
                return NotFound(new { message = "Interview round not found" });
            }

            return Ok(interviewRound);
        }

        // GET: api/Interview/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllInterviewRounds()
        {
            var rounds = await _interviewService.GetAllInterviewRoundsAsync();
            return Ok(rounds);
        }

        // GET: api/Interview/application/{applicationId}
        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetInterviewRoundsByApplication(Guid applicationId)
        {
            var rounds = await _interviewService.GetInterviewRoundsByApplicationAsync(applicationId);
            return Ok(rounds);
        }

        // GET: api/Interview/my-schedule
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userId = GetCurrentUserId();
            var schedule = await _interviewService.GetInterviewerScheduleAsync(userId);
            return Ok(schedule);
        }

        // PUT: api/Interview/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateInterviewRound([FromBody] UpdateInterviewRoundDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _interviewService.UpdateInterviewRoundAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // POST: api/Interview/{interviewRoundId}/add-participant
        [HttpPost("{interviewRoundId}/add-participant")]
        public async Task<IActionResult> AddParticipant(Guid interviewRoundId, [FromBody] InterviewParticipantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _interviewService.AddParticipantAsync(interviewRoundId, dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // POST: api/Interview/submit-feedback
        [HttpPost("submit-feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] CreateInterviewFeedbackDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _interviewService.SubmitFeedbackAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                feedbackId = result.FeedbackId
            });
        }

        // PUT: api/Interview/update-feedback
        [HttpPut("update-feedback")]
        public async Task<IActionResult> UpdateFeedback([FromBody] UpdateInterviewFeedbackDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _interviewService.UpdateFeedbackAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // GET: api/Interview/{interviewRoundId}/feedbacks
        [HttpGet("{interviewRoundId}/feedbacks")]
        public async Task<IActionResult> GetFeedbacksByInterviewRound(Guid interviewRoundId)
        {
            var feedbacks = await _interviewService.GetFeedbacksByInterviewRoundAsync(interviewRoundId);
            return Ok(feedbacks);
        }

        // GET: api/Interview/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var statistics = await _interviewService.GetInterviewStatisticsAsync();
            return Ok(statistics);
        }

        // DELETE: api/Interview/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInterviewRound(Guid id)
        {
            var result = await _interviewService.DeleteInterviewRoundAsync(id);

            if (!result)
            {
                return NotFound(new { message = "Interview round not found" });
            }

            return Ok(new { message = "Interview round deleted successfully" });
        }
    }
}