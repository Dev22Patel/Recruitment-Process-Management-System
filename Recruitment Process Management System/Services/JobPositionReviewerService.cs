using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class JobPositionReviewerService
    {
        private readonly IJobPositionReviewerRepository _jobPositionReviewerRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;

        public JobPositionReviewerService(
            IJobPositionReviewerRepository jobPositionReviewerRepository,
            IJobPositionRepository jobPositionRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository)
        {
            _jobPositionReviewerRepository = jobPositionReviewerRepository;
            _jobPositionRepository = jobPositionRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<(bool Success, string Message)> AssignReviewerToJobPositionAsync(
            AssignReviewerDto dto, Guid assignedBy)
        {
            try
            {
                // Validate job position exists
                var jobPosition = await _jobPositionRepository.GetJobPositionByIdAsync(dto.JobPositionId);
                if (jobPosition == null)
                    return (false, "Job position not found");

                // Validate reviewer exists
                var reviewer = await _userRepository.GetUserByIdAsync(dto.ReviewerId);
                if (reviewer == null)
                    return (false, "Reviewer not found");

                // Check if already assigned
                var isAlreadyAssigned = await _jobPositionReviewerRepository.IsReviewerAssignedToJobAsync(
                    dto.JobPositionId, dto.ReviewerId);

                if (isAlreadyAssigned)
                    return (false, "Reviewer is already assigned to this job position");

                // Create assignment
                var jobPositionReviewer = new JobPositionReviewer
                {
                    Id = Guid.NewGuid(),
                    JobPositionId = dto.JobPositionId,
                    ReviewerId = dto.ReviewerId,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _jobPositionReviewerRepository.AssignReviewerToJobAsync(jobPositionReviewer);



                return (true, "Reviewer assigned successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error assigning reviewer: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RemoveReviewerFromJobPositionAsync(Guid jobPositionReviewerId)
        {
            try
            {
                var result = await _jobPositionReviewerRepository.RemoveReviewerFromJobAsync(jobPositionReviewerId);

                if (!result)
                    return (false, "Job position reviewer assignment not found");

                return (true, "Reviewer removed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error removing reviewer: {ex.Message}");
            }
        }

        public async Task<List<JobPositionReviewerResponseDto>> GetReviewersForJobPositionAsync(Guid jobPositionId)
        {
            var reviewers = await _jobPositionReviewerRepository.GetReviewersByJobPositionAsync(jobPositionId);

            return reviewers.Select(r => new JobPositionReviewerResponseDto
            {
                Id = r.Id,
                JobPositionId = r.JobPositionId,
                JobTitle = r.JobPosition?.Title,
                ReviewerId = r.ReviewerId,
                ReviewerName = $"{r.Reviewer?.FirstName} {r.Reviewer?.LastName}",
                ReviewerEmail = r.Reviewer?.Email,
                AssignedAt = r.AssignedAt,
                AssignedByName = $"{r.Assigner?.FirstName} {r.Assigner?.LastName}",
                IsActive = r.IsActive
            }).ToList();
        }

        public async Task<List<JobPositionReviewerResponseDto>> GetMyAssignedJobPositionsAsync(Guid reviewerId)
        {
            var assignments = await _jobPositionReviewerRepository.GetJobPositionsByReviewerAsync(reviewerId);

            return assignments.Select(a => new JobPositionReviewerResponseDto
            {
                Id = a.Id,
                JobPositionId = a.JobPositionId,
                JobTitle = a.JobPosition?.Title,
                ReviewerId = a.ReviewerId,
                ReviewerName = $"{a.Reviewer?.FirstName} {a.Reviewer?.LastName}",
                ReviewerEmail = a.Reviewer?.Email,
                AssignedAt = a.AssignedAt,
                AssignedByName = $"{a.Assigner?.FirstName} {a.Assigner?.LastName}",
                IsActive = a.IsActive
            }).ToList();
        }

        public async Task<(bool Success, string Message)> BulkAssignReviewersAsync(
            BulkAssignReviewersDto dto, Guid assignedBy)
        {
            try
            {
                // Validate job position
                var jobPosition = await _jobPositionRepository.GetJobPositionByIdAsync(dto.JobPositionId);
                if (jobPosition == null)
                    return (false, "Job position not found");

                var successCount = 0;
                var errorMessages = new List<string>();

                foreach (var reviewerId in dto.ReviewerIds)
                {
                    // Check if already assigned
                    var isAlreadyAssigned = await _jobPositionReviewerRepository.IsReviewerAssignedToJobAsync(
                        dto.JobPositionId, reviewerId);

                    if (isAlreadyAssigned)
                    {
                        errorMessages.Add($"Reviewer {reviewerId} is already assigned");
                        continue;
                    }

                    // Validate reviewer exists
                    var reviewer = await _userRepository.GetUserByIdAsync(reviewerId);
                    if (reviewer == null)
                    {
                        errorMessages.Add($"Reviewer {reviewerId} not found");
                        continue;
                    }

                    // Create assignment
                    var jobPositionReviewer = new JobPositionReviewer
                    {
                        Id = Guid.NewGuid(),
                        JobPositionId = dto.JobPositionId,
                        ReviewerId = reviewerId,
                        AssignedBy = assignedBy,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _jobPositionReviewerRepository.AssignReviewerToJobAsync(jobPositionReviewer);



                    successCount++;
                }

                if (successCount == 0)
                    return (false, $"No reviewers assigned. Errors: {string.Join(", ", errorMessages)}");

                var message = successCount == dto.ReviewerIds.Count
                    ? $"All {successCount} reviewers assigned successfully"
                    : $"{successCount} reviewers assigned. Errors: {string.Join(", ", errorMessages)}";

                return (true, message);
            }
            catch (Exception ex)
            {
                return (false, $"Error bulk assigning reviewers: {ex.Message}");
            }
        }
    }
}