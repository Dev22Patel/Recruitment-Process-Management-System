using MimeKit.Cryptography;
using Recruitment_Process_Management_System.Models.DTOs.Application_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Implementations;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class ApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly EmailService _emailService;
        private readonly IUserRepository _userRepository;

        public ApplicationService(
            IApplicationRepository applicationRepository,
            ICandidateRepository candidateRepository,
            IJobPositionRepository jobPositionRepository,
            INotificationRepository notificationRepository,
            EmailService emailService,
            IUserRepository userRepository
        )
        {
            _applicationRepository = applicationRepository;
            _candidateRepository = candidateRepository;
            _jobPositionRepository = jobPositionRepository;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message, ApplicationResponseDto? Application)> CreateApplicationAsync(
            Guid userId, CreateApplicationDto dto)
        {
            // Get candidate by user ID
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null)
            {
                return (false, "Candidate profile not found", null);
            }

            // Check if profile is complete
            if (!candidate.IsProfileCompleted)
            {
                return (false, "Please complete your profile before applying", null);
            }

            // Check if job position exists and is active
            var jobPosition = await _jobPositionRepository.GetByIdAsync(dto.JobPositionId);
            if (jobPosition == null)
            {
                return (false, "Job position not found", null);
            }

            // Check if job is still open (StatusId = 1 for "Open")
            if (jobPosition.StatusId != 1)
            {
                return (false, "This job position is no longer accepting applications", null);
            }

            // Check if candidate has already applied
            var existingApplication = await _applicationRepository.ExistsAsync(candidate.Id, dto.JobPositionId);
            if (existingApplication)
            {
                return (false, "You have already applied for this position", null);
            }

            // Create application
            var application = new Application
            {
                CandidateId = candidate.Id,
                JobPositionId = dto.JobPositionId,
                StatusId = 4, // "Applied" status
                StatusReason = "Application submitted successfully"
            };

            var createdApplication = await _applicationRepository.CreateAsync(application);

            // Create notification for HR/Recruiters
            await CreateApplicationNotificationAsync(createdApplication, candidate, jobPosition);

            // Get full application details
            var applicationResponse = await GetApplicationByIdAsync(createdApplication.Id);

            return (true, "Application submitted successfully", applicationResponse);
        }

        public async Task<ApplicationResponseDto?> GetApplicationByIdAsync(Guid id)
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            return application == null ? null : MapToResponseDto(application);
        }

        public async Task<List<ApplicationResponseDto>> GetAllApplicationsAsync()
        {
            var applications = await _applicationRepository.GetAllAsync();
            return applications.Select(MapToResponseDto).ToList();
        }

        public async Task<List<CandidateApplicationHistoryDto>> GetCandidateApplicationsAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null) return new List<CandidateApplicationHistoryDto>();

            var applications = await _applicationRepository.GetByCandidateIdAsync(candidate.Id);
            return applications.Select(a => new CandidateApplicationHistoryDto
            {
                ApplicationId = a.Id,
                JobPositionId = a.JobPositionId,
                JobTitle = a.JobPosition?.Title,
                Department = a.JobPosition?.Department,
                Location = a.JobPosition?.Location,
                ApplicationDate = a.ApplicationDate,
                CurrentStatus = a.Status?.StatusName,
                StatusReason = a.StatusReason
            }).ToList();
        }

        public async Task<List<ApplicationResponseDto>> GetApplicationsByJobPositionAsync(Guid jobPositionId)
        {
            var applications = await _applicationRepository.GetByJobPositionIdAsync(jobPositionId);
            return applications.Select(MapToResponseDto).ToList();
        }

        public async Task<(bool Success, string Message)> UpdateApplicationStatusAsync(
            UpdateApplicationStatusDto dto, Guid updatedBy)
        {
            var application = await _applicationRepository.GetByIdAsync(dto.ApplicationId);
            if (application == null)
            {
                return (false, "Application not found");
            }

            application.StatusId = dto.StatusId;
            application.StatusReason = dto.StatusReason;

            await _applicationRepository.UpdateAsync(application);

            // Create notification for candidate
            await CreateStatusUpdateNotificationAsync(application, updatedBy);

            return (true, "Application status updated successfully");
        }

        public async Task<ApplicationStatisticsDto> GetApplicationStatisticsAsync(Guid? jobPositionId = null)
        {
            List<Application> applications;

            if (jobPositionId.HasValue)
            {
                applications = await _applicationRepository.GetByJobPositionIdAsync(jobPositionId.Value);
            }
            else
            {
                applications = await _applicationRepository.GetAllAsync();
            }

            return new ApplicationStatisticsDto
            {
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.StatusId == 4), // Applied
                InScreeningApplications = applications.Count(a => a.StatusId == 5), // Screening
                InInterviewApplications = applications.Count(a => a.StatusId == 6), // Interview
                SelectedApplications = applications.Count(a => a.StatusId == 7), // Selected
                RejectedApplications = applications.Count(a => a.StatusId == 8) // Rejected
            };
        }

        public async Task<bool> WithdrawApplicationAsync(Guid applicationId, Guid userId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null) return false;

            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null || application.CandidateId != candidate.Id)
            {
                return false; // Not authorized
            }

            // Only allow withdrawal if status is "Applied" or "Screening"
            if (application.StatusId != 4 && application.StatusId != 5)
            {
                return false;
            }

            application.StatusId = 8; // Rejected
            application.StatusReason = "Withdrawn by candidate";
            await _applicationRepository.UpdateAsync(application);

            return true;
        }

        // Helper methods
        private ApplicationResponseDto MapToResponseDto(Application application)
        {
            var candidateSkills = application.Candidate?.CandidateSkills?
                .Select(cs => cs.Skill?.SkillName ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? new List<string>();

            var requiredSkills = application.JobPosition?.JobSkillRequirements?
                .Where(jsr => jsr.IsRequired)
                .Select(jsr => jsr.Skill?.SkillName ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? new List<string>();

            var matchingSkills = candidateSkills.Intersect(requiredSkills).Count();

            return new ApplicationResponseDto
            {
                Id = application.Id,
                CandidateId = application.CandidateId,
                CandidateName = application.Candidate?.User != null 
                    ? $"{application.Candidate.User.FirstName} {application.Candidate.User.LastName}" 
                    : null,
                CandidateEmail = application.Candidate?.User?.Email,
                JobPositionId = application.JobPositionId,
                JobTitle = application.JobPosition?.Title,
                Department = application.JobPosition?.Department,
                Location = application.JobPosition?.Location,
                ApplicationDate = application.ApplicationDate,
                StatusId = application.StatusId,
                StatusName = application.Status?.StatusName,
                StatusReason = application.StatusReason,
                CreatedAt = application.CreatedAt,
                TotalExperience = application.Candidate?.TotalExperience,
                ExpectedSalary = application.Candidate?.ExpectedSalary,
                NoticePeriod = application.Candidate?.NoticePeriod,
                ResumeFilePath = application.Candidate?.ResumeFilePath,
                CandidateSkills = candidateSkills,
                RequiredSkills = requiredSkills,
                MatchingSkillsCount = matchingSkills
            };
        }

        private async Task CreateApplicationNotificationAsync(
            Application application, Candidate candidate, JobPosition jobPosition)
        {
            try
            {

                var createdByUser = await _userRepository.GetByIdAsync(jobPosition.CreatedBy);


                //we can give in app notification also so for that purpose i have created this below notification object

                //var notification = new Notification
                //{
                //    UserId = jobPosition.CreatedBy,
                //    Title = "New Job Application",
                //    Message = $"{candidate.User?.FirstName} {candidate.User?.LastName} has applied for {jobPosition.Title}",
                //    RelatedEntityType = "Application",
                //    RelatedEntityId = application.Id,
                //    CreatedAt = DateTime.UtcNow
                //};

                await _emailService.QueueEmailAsync(
                    createdByUser.Email, 
                    "New Job Application",
                    $"{candidate.User?.FirstName} {candidate.User?.LastName} has applied for {jobPosition.Title}"
                );

            }
            catch
            {
                // Log error but don't fail the application process
            }
        }

        private async Task CreateStatusUpdateNotificationAsync(Application application, Guid updatedBy)
        {
            try
            {
                if (application.Candidate?.UserId == null)
                    return;

                //we can give in app notification also so for that purpose i have created this below notification object

                //var notification = new Notification
                //{
                //    UserId = application.Candidate.UserId,
                //    Title = "Application Status Updated",
                //    Message = $"Your application for {application.JobPosition?.Title} has been updated to {application.Status?.StatusName}",
                //    RelatedEntityType = "Application",
                //    RelatedEntityId = application.Id,
                //    CreatedAt = DateTime.UtcNow
                //};

                var candidateUser = await _userRepository.GetByIdAsync(application.Candidate.UserId);

                if (candidateUser != null)
                {
                    await _emailService.QueueEmailAsync(
                        candidateUser.Email,
                        "Application Status Updated",
                        $"Dear {candidateUser.FirstName}, your application for {application.JobPosition?.Title} has been updated to {application.Status?.StatusName}."
                    );
                }
            }
            catch (Exception ex)
            {
                // TODO: log ex (avoid swallowing errors silently)
            }
        }

    }
}