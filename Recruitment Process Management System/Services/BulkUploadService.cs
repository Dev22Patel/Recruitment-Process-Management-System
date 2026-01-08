using Hangfire;
using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using System.Text;

namespace Recruitment_Process_Management_System.Services
{
    public class BulkUploadService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IBulkUploadRepository _bulkUploadRepository;
        private readonly EmailService _emailService;
        private readonly ExcelParserService _excelParser;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BulkUploadService> _logger;

        public BulkUploadService(
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            ISkillRepository skillRepository,
            IBulkUploadRepository bulkUploadRepository,
            EmailService emailService,
            ExcelParserService excelParser,
            ApplicationDbContext context,
            ILogger<BulkUploadService> logger)
        {
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _skillRepository = skillRepository;
            _bulkUploadRepository = bulkUploadRepository;
            _emailService = emailService;
            _excelParser = excelParser;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initiates bulk upload process (synchronous parsing, async processing)
        /// </summary>
        public async Task<(bool Success, string Message, Guid? BulkUploadId)> InitiateBulkUploadAsync(
            IFormFile file,
            Guid uploadedBy)
        {
            try
            {
                // 1. Validate file
                if (file == null || file.Length == 0)
                    return (false, "File is empty", null);

                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return (false, "Invalid file format. Only Excel files (.xlsx, .xls) are allowed", null);

                // Check file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                    return (false, "File size exceeds 10MB limit", null);

                // 2. Parse Excel
                List<CandidateExcelRow> candidates;
                using (var stream = file.OpenReadStream())
                {
                    candidates = await _excelParser.ParseCandidateExcelAsync(stream);
                }

                if (!candidates.Any())
                    return (false, "No data found in Excel file", null);

                _logger.LogInformation($"Parsed {candidates.Count} rows from Excel");

                // 3. Get "Processing" status
                var processingStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.EntityType == "BulkUpload" && s.StatusName == "Processing");

                if (processingStatus == null)
                {
                    // Create status if doesn't exist
                    processingStatus = new Status
                    {
                        EntityType = "BulkUpload",
                        StatusName = "Processing",
                        IsActive = true
                    };
                    _context.Statuses.Add(processingStatus);
                    await _context.SaveChangesAsync();
                }

                // 4. Create BulkUpload record
                var bulkUpload = new BulkUpload
                {
                    Id = Guid.NewGuid(),
                    FileName = file.FileName,
                    UploadType = "Excel",
                    TotalRecords = candidates.Count,
                    SuccessfulRecords = 0,
                    FailedRecords = 0,
                    StatusId = processingStatus.Id,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };

                await _bulkUploadRepository.CreateAsync(bulkUpload);

                _logger.LogInformation($"Created BulkUpload record: {bulkUpload.Id}");

                // 5. Queue background job using Hangfire
                BackgroundJob.Enqueue(() => ProcessCandidatesAsync(bulkUpload.Id, candidates));

                return (true, "Upload initiated successfully. Processing in background.", bulkUpload.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bulk upload initiation failed: {ex.Message}", ex);
                return (false, $"Upload failed: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Background job to process candidates
        /// This method is executed by Hangfire
        /// </summary>
        [AutomaticRetry(Attempts = 0)] // Don't retry on failure
        public async Task ProcessCandidatesAsync(Guid bulkUploadId, List<CandidateExcelRow> candidates)
        {
            var successCount = 0;
            var failedCount = 0;
            var errorLog = new StringBuilder();

            try
            {
                _logger.LogInformation($"Starting processing for BulkUpload: {bulkUploadId}");

                // Get candidate role
                var candidateRole = await _roleRepository.GetByNameAsync("Candidate");
                if (candidateRole == null)
                {
                    throw new Exception("Candidate role not found in database");
                }

                foreach (var row in candidates)
                {
                    try
                    {
                        // Skip invalid rows
                        if (!row.IsValid)
                        {
                            failedCount++;
                            errorLog.AppendLine($"Row {row.RowNumber}: {string.Join(", ", row.Errors)}");
                            continue;
                        }

                        // Check duplicate email
                        if (await _userRepository.EmailExistsAsync(row.Email))
                        {
                            failedCount++;
                            errorLog.AppendLine($"Row {row.RowNumber}: Email '{row.Email}' already exists");
                            continue;
                        }

                        // Generate random password
                        var tempPassword = GenerateRandomPassword(10);
                        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                        // Create User
                        var user = new User
                        {
                            Id = Guid.NewGuid(),
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            Email = row.Email,
                            PhoneNumber = row.PhoneNumber,
                            PasswordHash = passwordHash,
                            UserType = "Candidate",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var createdUser = await _userRepository.CreateAsync(user);

                        // Create Candidate
                        var candidate = new Candidate
                        {
                            Id = Guid.NewGuid(),
                            UserId = createdUser.Id,
                            User = createdUser,
                            CurrentLocation = row.CurrentLocation,
                            CollegeName = row.CollegeName,
                            Degree = row.Degree,
                            GraduationYear = row.GraduationYear,
                            TotalExperience = row.TotalExperience,
                            CurrentCompany = row.CurrentCompany,
                            Source = "Excel Import",
                            IsProfileCompleted = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _candidateRepository.CreateAsync(candidate);

                        // Assign Candidate role
                        var userRole = new UserRole
                        {
                            UserId = createdUser.Id,
                            RoleId = candidateRole.Id,
                            AssignedAt = DateTime.UtcNow
                        };
                        await _userRoleRepository.CreateAsync(userRole);

                        // Process Skills if provided
                        if (!string.IsNullOrWhiteSpace(row.Skills))
                        {
                            await ProcessSkillsAsync(candidate.Id, row.Skills);
                        }

                        // Send welcome email with credentials
                        var emailBody = GenerateWelcomeEmail(row.FirstName, row.LastName, row.Email, tempPassword);
                        await _emailService.QueueEmailAsync(row.Email, "Welcome to Recruitment Portal - Your Account Credentials", emailBody);

                        successCount++;
                        _logger.LogInformation($"Successfully processed row {row.RowNumber}: {row.Email}");
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        errorLog.AppendLine($"Row {row.RowNumber}: {ex.Message}");
                        _logger.LogError($"Failed to process row {row.RowNumber} ({row.Email}): {ex.Message}");
                    }
                }

                // Update BulkUpload record with results
                await UpdateBulkUploadStatusAsync(bulkUploadId, successCount, failedCount, errorLog.ToString(), "Completed");

                _logger.LogInformation($"Completed processing for BulkUpload: {bulkUploadId}. Success: {successCount}, Failed: {failedCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in ProcessCandidatesAsync for BulkUpload {bulkUploadId}: {ex.Message}", ex);
                await UpdateBulkUploadStatusAsync(bulkUploadId, successCount, failedCount,
                    $"Critical error: {ex.Message}\n{errorLog}", "Failed");
            }
        }

        /// <summary>
        /// Get bulk upload status
        /// </summary>
        public async Task<BulkUploadStatusDto?> GetBulkUploadStatusAsync(Guid bulkUploadId)
        {
            var bulkUpload = await _bulkUploadRepository.GetByIdAsync(bulkUploadId);

            if (bulkUpload == null)
                return null;

            return new BulkUploadStatusDto
            {
                Id = bulkUpload.Id,
                FileName = bulkUpload.FileName,
                UploadType = bulkUpload.UploadType,
                TotalRecords = bulkUpload.TotalRecords,
                SuccessfulRecords = bulkUpload.SuccessfulRecords,
                FailedRecords = bulkUpload.FailedRecords,
                Status = bulkUpload.Status.StatusName,
                ErrorLog = bulkUpload.ErrorLog,
                UploadedAt = bulkUpload.UploadedAt,
                UploadedByName = $"{bulkUpload.UploadedByUser.FirstName} {bulkUpload.UploadedByUser.LastName}"
            };
        }

        /// <summary>
        /// Get all bulk uploads with pagination
        /// </summary>
        public async Task<List<BulkUploadStatusDto>> GetAllBulkUploadsAsync(int pageNumber = 1, int pageSize = 20)
        {
            var bulkUploads = await _bulkUploadRepository.GetAllAsync(pageNumber, pageSize);

            return bulkUploads.Select(b => new BulkUploadStatusDto
            {
                Id = b.Id,
                FileName = b.FileName,
                UploadType = b.UploadType,
                TotalRecords = b.TotalRecords,
                SuccessfulRecords = b.SuccessfulRecords,
                FailedRecords = b.FailedRecords,
                Status = b.Status.StatusName,
                ErrorLog = b.ErrorLog,
                UploadedAt = b.UploadedAt,
                UploadedByName = $"{b.UploadedByUser.FirstName} {b.UploadedByUser.LastName}"
            }).ToList();
        }

        /// <summary>
        /// Generate Excel template for download
        /// </summary>
        public async Task<MemoryStream> GenerateExcelTemplateAsync()
        {
            return await _excelParser.GenerateExcelTemplateAsync();
        }

        #region Private Helper Methods

        private async Task ProcessSkillsAsync(Guid candidateId, string skillsString)
        {
            try
            {
                var skillNames = skillsString.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s => s.Trim())
                                             .Where(s => !string.IsNullOrWhiteSpace(s))
                                             .Distinct();

                foreach (var skillName in skillNames)
                {
                    // Check if skill exists
                    var skill = await _skillRepository.GetByNameAsync(skillName);

                    if (skill == null)
                    {
                        // Create new skill
                        skill = new Skill
                        {
                            Id = Guid.NewGuid(),
                            SkillName = skillName,
                            Category = "Technical", // Default category
                            IsActive = true
                        };
                        await _skillRepository.CreateAsync(skill);
                    }

                    // Check if candidate already has this skill
                    var existingCandidateSkill = await _context.CandidateSkills
                        .FirstOrDefaultAsync(cs => cs.CandidateId == candidateId && cs.SkillId == skill.Id);

                    if (existingCandidateSkill == null)
                    {
                        // Link skill to candidate
                        var candidateSkill = new CandidateSkill
                        {
                            Id = Guid.NewGuid(),
                            CandidateId = candidateId,
                            SkillId = skill.Id,
                            YearsOfExperience = 0 // Default for freshers
                        };

                        _context.CandidateSkills.Add(candidateSkill);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing skills for candidate {candidateId}: {ex.Message}");
            }
        }

        private async Task UpdateBulkUploadStatusAsync(
            Guid bulkUploadId,
            int successCount,
            int failedCount,
            string errorLog,
            string statusName)
        {
            try
            {
                var bulkUpload = await _bulkUploadRepository.GetByIdAsync(bulkUploadId);
                if (bulkUpload == null)
                    return;

                bulkUpload.SuccessfulRecords = successCount;
                bulkUpload.FailedRecords = failedCount;
                bulkUpload.ErrorLog = errorLog;

                // Get status
                var status = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.EntityType == "BulkUpload" && s.StatusName == statusName);

                if (status != null)
                {
                    bulkUpload.StatusId = status.Id;
                }

                await _bulkUploadRepository.UpdateAsync(bulkUpload);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating BulkUpload status: {ex.Message}");
            }
        }

        private string GenerateRandomPassword(int length = 10)
        {
            const string upperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@#$!";

            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one of each type
            password.Append(upperCase[random.Next(upperCase.Length)]);
            password.Append(lowerCase[random.Next(lowerCase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Fill remaining with random characters
            const string allChars = upperCase + lowerCase + digits + special;
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            var shuffled = password.ToString().ToCharArray();
            for (int i = shuffled.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            return new string(shuffled);
        }

        private string GenerateWelcomeEmail(string firstName, string lastName, string email, string password)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .credentials {{ background: white; padding: 20px; border-left: 4px solid #667eea; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to Our Recruitment Portal!</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName} {lastName},</h2>
            <p>Your account has been successfully created for the campus placement process. We're excited to have you on board!</p>
            
            <div class='credentials'>
                <h3>📧 Your Login Credentials:</h3>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Temporary Password:</strong> <code style='background: #f0f0f0; padding: 5px 10px; border-radius: 3px; font-size: 16px;'>{password}</code></p>
            </div>
            
            <p><strong>⚠️ Important:</strong></p>
            <ul>
                <li>Please login with the credentials given above</li>
                <li>Complete your profile with all required details </li>
                <li>Upload your resume and relevant documents</li>
            </ul>
            
            <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
            
            <p>Best regards,<br>
            <strong>HR Team</strong></p>
        </div>
        <div class='footer'>
            <p>This is an automated email. Please do not reply to this message.</p>
            <p>&copy; {DateTime.Now.Year} Recruitment Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}