using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using Recruitment_Process_Management_System.Services;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly ICandidateService _candidateService;

        public CandidateController(ICandidateRepository candidateRepository, ICandidateService candidateService)
        {
            _candidateRepository = candidateRepository;
            _candidateService = candidateService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCandidateProfile(Guid userId)
        {
            try
            {
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                var profileDto = _candidateService.MapToProfileDto(candidate);
                return Ok(profileDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the candidate profile.", Error = ex.Message });
            }
        }

        [HttpGet("{userId}/isComplete")]
        public async Task<IActionResult> IsCandidateProfileComplete(Guid userId)
        {
            try
            {
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                var isComplete = _candidateService.IsCandidateProfileComplete(candidate);
                return Ok(new { IsComplete = isComplete });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while checking profile completion.", Error = ex.Message });
            }
        }

        [HttpPut("{userId}/profile")]
        public async Task<IActionResult> UpdateCandidateProfile(Guid userId, [FromBody] UpdateCandidate updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedCandidate = await _candidateService.UpdateCandidateProfileAsync(userId, updateDto);
                if (updatedCandidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                var profileDto = _candidateService.MapToProfileDto(updatedCandidate);
                return Ok(new
                {
                    Message = "Candidate profile updated successfully.",
                    Data = profileDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the candidate profile.", Error = ex.Message });
            }
        }

        [HttpPatch("{userId}/profile-completion")]
        public async Task<IActionResult> UpdateProfileCompletionStatus(Guid userId, [FromBody] bool isCompleted)
        {
            try
            {
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                var success = await _candidateRepository.UpdateProfileCompletionStatusAsync(candidate.Id, isCompleted);
                if (!success)
                {
                    return StatusCode(500, new { Message = "Failed to update profile completion status." });
                }

                return Ok(new { Message = "Profile completion status updated successfully.", IsCompleted = isCompleted });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating profile completion status.", Error = ex.Message });
            }
        }

        [HttpGet("{userId}/validate-completion")]
        public async Task<IActionResult> ValidateProfileCompletion(Guid userId)
        {
            try
            {
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                var isComplete = _candidateService.ValidateProfileCompletion(candidate);

                // Update the database with the validation result
                await _candidateRepository.UpdateProfileCompletionStatusAsync(candidate.Id, isComplete);

                return Ok(new
                {
                    IsComplete = isComplete,
                    Message = isComplete ? "Profile is complete." : "Profile is incomplete. Please fill in all required fields."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while validating profile completion.", Error = ex.Message });
            }
        }
    }
}
