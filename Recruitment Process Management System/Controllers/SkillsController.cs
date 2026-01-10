using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Services;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly SkillService _skillService;

        public SkillController(SkillService skillService)
        {
            _skillService = skillService ?? throw new ArgumentNullException(nameof(skillService));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateSkill([FromBody] Skill skill)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdSkill = await _skillService.CreateSkillAsync(skill);
                return CreatedAtAction(nameof(GetSkillById), new { id = createdSkill.Id }, createdSkill);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating skill.", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin , Candidate")]
        public async Task<IActionResult> GetSkillById(Guid id)
        {
            try
            {
                var skill = await _skillService.GetSkillByIdAsync(id);
                if (skill == null)
                {
                    return NotFound(new { Message = "Skill not found." });
                }
                return Ok(skill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving skill.", Error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Candidate , Admin,Recruiter,Hr,Reviewer,Interviewer")]
        public async Task<IActionResult> GetAllSkills()
        {
            try
            {
                var skills = await _skillService.GetAllSkillsAsync();
                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving skills.", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] Skill skill)
        {
            try
            {
                if (!ModelState.IsValid || skill.Id != id)
                {
                    return BadRequest(ModelState);
                }

                var updatedSkill = await _skillService.UpdateSkillAsync(skill);
                return Ok(updatedSkill);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Skill not found." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error updating skill.", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSkill(Guid id)
        {
            try
            {
                var success = await _skillService.DeleteSkillAsync(id);
                if (!success)
                {
                    return NotFound(new { Message = "Skill not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error deleting skill.", Error = ex.Message });
            }
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateSkill(Guid id)
        {
            try
            {
                var success = await _skillService.DeactivateSkillAsync(id);
                if (!success)
                {
                    return NotFound(new { Message = "Skill not found." });
                }
                return Ok(new { Message = "Skill deactivated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error deactivating skill.", Error = ex.Message });
            }
        }
    }
}
