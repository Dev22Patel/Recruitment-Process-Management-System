using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null)
            {
                return NotFound(new { Message = "Candidate profile not found." });
            }
            return Ok(candidate);
        }


        [HttpGet("{userId}/isComplete")]
        public async Task<IActionResult> IsCandidateProfileComplete(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null)
            {
                return NotFound(new { Message = "Candidate profile not found." });
            }
            var isComplete = _candidateService.IsCandidateProfileComplete(candidate);
            return Ok(new { IsComplete = isComplete });
        }
    }
}
