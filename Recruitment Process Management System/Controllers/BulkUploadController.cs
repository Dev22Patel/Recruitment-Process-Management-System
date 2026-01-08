using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Services;
using System.Security.Claims;

namespace Recruitment_Process_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class BulkUploadController : ControllerBase
    {
        private readonly BulkUploadService _bulkUploadService;
        private readonly ILogger<BulkUploadController> _logger;

        public BulkUploadController(
            BulkUploadService bulkUploadService,
            ILogger<BulkUploadController> logger)
        {
            _bulkUploadService = bulkUploadService;
            _logger = logger;
        }

        /// <summary>
        /// Upload Excel file with candidate data
        /// Only accessible by Admin and HR roles
        /// </summary>
        [HttpPost("upload-excel")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin,HR")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest(new { Message = "No file uploaded" });

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { Message = "User not authenticated" });

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                    return BadRequest(new { Message = "Invalid user ID" });

                var (success, message, bulkUploadId) = await _bulkUploadService
                    .InitiateBulkUploadAsync(file, userId);

                if (!success)
                    return BadRequest(new { Message = message });

                return Ok(new
                {
                    Message = message,
                    BulkUploadId = bulkUploadId,
                    Note = "Processing in background. Check status using the provided ID."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UploadExcel: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        /// <summary>
        /// Get status of a specific bulk upload
        /// </summary>
        [HttpGet("status/{bulkUploadId}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetUploadStatus(Guid bulkUploadId)
        {
            try
            {
                var status = await _bulkUploadService.GetBulkUploadStatusAsync(bulkUploadId);

                if (status == null)
                    return NotFound(new { Message = "Bulk upload not found" });

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUploadStatus: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all bulk uploads with pagination
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAllUploads(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var uploads = await _bulkUploadService.GetAllBulkUploadsAsync(pageNumber, pageSize);

                return Ok(new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = uploads
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllUploads: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Download Excel template for bulk upload
        /// </summary>
        [HttpGet("template")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var stream = await _bulkUploadService.GenerateExcelTemplateAsync();

                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Candidate_Upload_Template_{DateTime.Now:yyyyMMdd}.xlsx"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DownloadTemplate: {ex.Message}");
                return StatusCode(500, new { Message = "Failed to generate template" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "Bulk Upload API",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}