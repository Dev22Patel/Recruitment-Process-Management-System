using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using Recruitment_Process_Management_System.Services;
using Recruitment_Process_Management_System.Services.CloudinaryService;

namespace Recruitment_Process_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly ICandidateService _candidateService;
        private readonly ICloudinaryService _cloudinaryService;

        public CandidateController(
            ICandidateRepository candidateRepository,
            ICandidateService candidateService,
            ICloudinaryService cloudinaryService)
        {
            _candidateRepository = candidateRepository;
            _candidateService = candidateService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// Get candidate profile by user ID
        /// </summary>
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

        /// <summary>
        /// Upload candidate resume to Cloudinary
        /// </summary>
        [HttpPost("{userId}/upload-resume")]
        public async Task<IActionResult> UploadResume(Guid userId, IFormFile resumeFile)
        {
            try
            {
                // Validate file presence
                if (resumeFile == null || resumeFile.Length == 0)
                {
                    return BadRequest(new { Message = "No file provided." });
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(resumeFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new
                    {
                        Message = "Only PDF, DOC, and DOCX files are allowed.",
                        AllowedFormats = allowedExtensions
                    });
                }

                // Validate file size (max 5MB)
                const long maxSizeInBytes = 5 * 1024 * 1024;
                if (resumeFile.Length > maxSizeInBytes)
                {
                    return BadRequest(new
                    {
                        Message = "File size must not exceed 5MB.",
                        MaxSizeInMB = maxSizeInBytes / (1024 * 1024)
                    });
                }

                // Get candidate
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                // Delete old resume from Cloudinary if exists
                if (!string.IsNullOrEmpty(candidate.ResumeFilePath))
                {
                    var oldPublicId = ExtractPublicIdFromUrl(candidate.ResumeFilePath);
                    if (!string.IsNullOrEmpty(oldPublicId))
                    {
                        await _cloudinaryService.DeleteResumeAsync(oldPublicId);
                    }
                }

                // Upload new resume to Cloudinary
                string resumeUrl;
                using (var stream = resumeFile.OpenReadStream())
                {
                    resumeUrl = await _cloudinaryService.UploadResumeAsync(stream, resumeFile.FileName);
                }

                // Save URL to database
                candidate.ResumeFilePath = resumeUrl;
                await _candidateRepository.UpdateAsync(candidate);

                return Ok(new
                {
                    Message = "Resume uploaded successfully.",
                    ResumeUrl = resumeUrl,
                    FileName = resumeFile.FileName,
                    FileSizeInMB = (resumeFile.Length / (1024.0 * 1024.0)).ToString("F2")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while uploading resume.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete candidate resume from Cloudinary
        /// </summary>
        [HttpDelete("{userId}/resume")]
        public async Task<IActionResult> DeleteResume(Guid userId)
        {
            try
            {
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null)
                {
                    return NotFound(new { Message = "Candidate profile not found." });
                }

                if (string.IsNullOrEmpty(candidate.ResumeFilePath))
                {
                    return BadRequest(new { Message = "No resume to delete." });
                }

                // Delete from Cloudinary
                var publicId = ExtractPublicIdFromUrl(candidate.ResumeFilePath);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _cloudinaryService.DeleteResumeAsync(publicId);
                }

                // Clear from database
                candidate.ResumeFilePath = null;
                await _candidateRepository.UpdateAsync(candidate);

                return Ok(new { Message = "Resume deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while deleting resume.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get resume URL for a candidate
        /// </summary>
        [HttpGet("{userId}/resume")]
        public async Task<IActionResult> GetResumeUrl(Guid userId)
        {
            try
            {
                var candidate = await _candidateRepository.GetByUserIdAsync(userId);
                if (candidate == null || string.IsNullOrEmpty(candidate.ResumeFilePath))
                {
                    return NotFound(new { Message = "Resume not found." });
                }

                return Ok(new { ResumeUrl = candidate.ResumeFilePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving resume.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Check if candidate profile is complete
        /// </summary>
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

        /// <summary>
        /// Update candidate profile information
        /// </summary>
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

        /// <summary>
        /// Update profile completion status
        /// </summary>
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

        /// <summary>
        /// Validate profile completion based on required fields
        /// </summary>
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

        /// <summary>
        /// Helper method to extract public ID from Cloudinary URL
        /// </summary>
        private string ExtractPublicIdFromUrl(string url)
        {
            try
            {
                // Cloudinary URL format: https://res.cloudinary.com/[cloud-name]/image/upload/[public-id]
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                // Find the "upload" segment and get everything after it
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
                {
                    // Join all segments after "upload" to get public ID (handles nested folders)
                    var publicId = string.Join("/", segments, uploadIndex + 1, segments.Length - uploadIndex - 1);
                    return publicId;
                }

                return url;
            }
            catch
            {
                return url;
            }
        }
    }
}