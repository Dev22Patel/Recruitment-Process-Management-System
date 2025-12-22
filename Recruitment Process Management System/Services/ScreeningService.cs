using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class ScreeningService
    {
        private readonly IScreeningReviewRepository _screeningReviewRepository;
        private readonly IReviewerSkillVerificationRepository _skillVerificationRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly INotificationRepository _notificationRepository;

        public ScreeningService(
            IScreeningReviewRepository screeningReviewRepository,
            IReviewerSkillVerificationRepository skillVerificationRepository,
            IApplicationRepository applicationRepository,
            ICandidateRepository candidateRepository,
            IJobPositionRepository jobPositionRepository,
            INotificationRepository notificationRepository)
        {
            _screeningReviewRepository = screeningReviewRepository;
            _skillVerificationRepository = skillVerificationRepository;
            _applicationRepository = applicationRepository;
            _candidateRepository = candidateRepository;
            _jobPositionRepository = jobPositionRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<(bool Success, string Message, ScreeningReviewResponseDto? ScreeningReview)> CreateScreeningReviewAsync(
            CreateScreeningReviewDto dto, Guid reviewerId)
        {
            try
            {
                // Validate application exists
                var application = await _applicationRepository.GetByIdAsync(dto.ApplicationId);
                if (application == null)
                    return (false, "Application not found", null);

                // Check if already screened
                var existingReview = await _screeningReviewRepository.GetScreeningReviewByApplicationIdAsync(dto.ApplicationId);
                if (existingReview != null)
                    return (false, "This application has already been screened", null);

                // Determine status based on recommendation
                int statusId = dto.IsRecommendedForInterview ? 16 : 17; // 16=Approved, 17=Rejected

                // Create screening review
                var screeningReview = new ScreeningReview
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = dto.ApplicationId,
                    ReviewedBy = reviewerId,
                    ReviewDate = DateTime.UtcNow,
                    StatusId = statusId,
                    Rating = dto.Rating,
                    Comments = dto.Comments,
                    IsRecommendedForInterview = dto.IsRecommendedForInterview,
                    CreatedAt = DateTime.UtcNow
                };

                var createdReview = await _screeningReviewRepository.CreateScreeningReviewAsync(screeningReview);

                // Create skill verifications if provided
                if (dto.VerifiedSkills != null && dto.VerifiedSkills.Any())
                {
                    var skillVerifications = dto.VerifiedSkills.Select(sv => new ReviewerSkillVerification
                    {
                        Id = Guid.NewGuid(),
                        ScreeningReviewId = createdReview.Id,
                        CandidateSkillId = sv.CandidateSkillId,
                        IsVerified = sv.IsVerified,
                        VerifiedYearsOfExperience = sv.VerifiedYears,
                        VerifiedBy = reviewerId,
                        Comments = sv.Comments,
                        VerifiedAt = DateTime.UtcNow
                    }).ToList();

                    await _skillVerificationRepository.BulkCreateSkillVerificationsAsync(skillVerifications);
                }



                // Update application status
                if (dto.IsRecommendedForInterview)
                {
                    // Move to Interview stage (StatusId = 6)
                    application.StatusId = 6;
                    application.StatusReason = "Approved in screening - Ready for interview";
                }
                else
                {
                    // Reject application (StatusId = 8)
                    application.StatusId = 8;
                    application.StatusReason = $"Rejected in screening - {dto.Comments}";
                }

                await _applicationRepository.UpdateAsync(application);



                // Return response
                var response = await MapToResponseDto(createdReview.Id);
                return (true, "Screening review created successfully", response);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating screening review: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateScreeningReviewAsync(UpdateScreeningReviewDto dto, Guid reviewerId)
        {
            try
            {
                var screeningReview = await _screeningReviewRepository.GetScreeningReviewByIdAsync(dto.ScreeningReviewId);
                if (screeningReview == null)
                    return (false, "Screening review not found");

                // Check if the reviewer is the owner
                if (screeningReview.ReviewedBy != reviewerId)
                    return (false, "You can only update your own screening reviews");

                // Update fields
                screeningReview.Rating = dto.Rating;
                screeningReview.Comments = dto.Comments;
                screeningReview.IsRecommendedForInterview = dto.IsRecommendedForInterview;
                screeningReview.StatusId = dto.IsRecommendedForInterview ? 16 : 17;
                screeningReview.UpdatedAt = DateTime.UtcNow;

                await _screeningReviewRepository.UpdateScreeningReviewAsync(screeningReview);

                // Update skill verifications if provided
                if (dto.VerifiedSkills != null && dto.VerifiedSkills.Any())
                {
                    foreach (var skillDto in dto.VerifiedSkills)
                    {
                        var existing = await _skillVerificationRepository.GetSkillVerificationAsync(
                            dto.ScreeningReviewId, skillDto.CandidateSkillId);

                        if (existing != null)
                        {
                            existing.IsVerified = skillDto.IsVerified;
                            existing.VerifiedYearsOfExperience = skillDto.VerifiedYears;
                            existing.Comments = skillDto.Comments;
                            await _skillVerificationRepository.UpdateSkillVerificationAsync(existing);
                        }
                        else
                        {
                            var newVerification = new ReviewerSkillVerification
                            {
                                Id = Guid.NewGuid(),
                                ScreeningReviewId = dto.ScreeningReviewId,
                                CandidateSkillId = skillDto.CandidateSkillId,
                                IsVerified = skillDto.IsVerified,
                                VerifiedYearsOfExperience = skillDto.VerifiedYears,
                                VerifiedBy = reviewerId,
                                Comments = skillDto.Comments,
                                VerifiedAt = DateTime.UtcNow
                            };
                            await _skillVerificationRepository.CreateSkillVerificationAsync(newVerification);
                        }
                    }
                }

                // Update application status
                var application = await _applicationRepository.GetByIdAsync(screeningReview.ApplicationId);
                if (application != null)
                {
                    if (dto.IsRecommendedForInterview)
                    {
                        application.StatusId = 6; // Interview
                        application.StatusReason = "Approved in screening - Ready for interview";
                    }
                    else
                    {
                        application.StatusId = 8; // Rejected
                        application.StatusReason = $"Rejected in screening - {dto.Comments}";
                    }
                    await _applicationRepository.UpdateAsync(application);
                }

                return (true, "Screening review updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating screening review: {ex.Message}");
            }
        }

        // Continuation of ScreeningService.cs

        public async Task<ScreeningReviewResponseDto?> GetScreeningReviewByIdAsync(Guid id)
        {
            return await MapToResponseDto(id);
        }

        public async Task<List<PendingScreeningResponseDto>> GetPendingScreeningsAsync(Guid reviewerId)
        {
            try
            {
                // Get job positions assigned to this reviewer
                var assignedJobIds = await _jobPositionRepository.GetJobPositionIdsByReviewerAsync(reviewerId);

                // Get applications in screening status (StatusId = 5) for these jobs
                var applications = await _applicationRepository.GetApplicationsByStatusAndJobsAsync(5, assignedJobIds);

                // Filter out applications that already have a screening review
                var pendingApplications = new List<Application>();
                foreach (var app in applications)
                {
                    var existingReview = await _screeningReviewRepository.GetScreeningReviewByApplicationIdAsync(app.Id);
                    if (existingReview == null)
                    {
                        pendingApplications.Add(app);
                    }
                }

                // Map to DTOs
                var result = new List<PendingScreeningResponseDto>();
                foreach (var app in pendingApplications)
                {
                    var candidate = await _candidateRepository.GetByIdAsync(app.CandidateId);
                    var jobPosition = await _jobPositionRepository.GetByIdAsync(app.JobPositionId);
                    var candidateSkills = await _candidateRepository.GetSkillsByIdAsync(app.CandidateId);
                    var jobSkillRequirements = await _jobPositionRepository.GetJobSkillRequirementsAsync(app.JobPositionId);


                    // Calculate matching skills
                    var matchingSkills = candidateSkills
                        .Count(cs => jobSkillRequirements.Any(jsr => jsr.SkillId == cs.SkillId));

                    result.Add(new PendingScreeningResponseDto
                    {
                        ApplicationId = app.Id,
                        CandidateName = $"{candidate?.User?.FirstName} {candidate?.User?.LastName}",
                        CandidateEmail = candidate?.User?.Email,
                        JobTitle = jobPosition?.Title,
                        ApplicationDate = app.ApplicationDate,
                        TotalExperience = candidate?.TotalExperience,
                        CurrentCompany = candidate?.CurrentCompany,
                        ResumeFilePath = candidate?.ResumeFilePath,
                        MatchingSkills = matchingSkills,
                        RequiredSkills = jobSkillRequirements.Count,
                        CandidateSkills = candidateSkills.Select(cs => new CandidateSkillDto
                        {
                            CandidateSkillId = cs.Id,
                            SkillId = cs.SkillId,
                            SkillName = cs.Skill?.SkillName,
                            YearsOfExperience = cs.YearsOfExperience,
                            IsVerified = false,
                            IsRequired = jobSkillRequirements.Any(jsr => jsr.SkillId == cs.SkillId && jsr.IsRequired)
                        }).ToList()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting pending screenings: {ex.Message}");
            }
        }

        public async Task<List<ScreeningReviewResponseDto>> GetScreeningsByApplicationAsync(Guid applicationId)
        {
            var reviews = await _screeningReviewRepository.GetScreeningReviewsByApplicationAsync(applicationId);
            var result = new List<ScreeningReviewResponseDto>();

            foreach (var review in reviews)
            {
                var dto = await MapToResponseDto(review.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }


        public async Task<ScreeningStatisticsDto> GetScreeningStatisticsAsync(Guid? reviewerId = null)
        {
            try
            {
                List<ScreeningReview> reviews;

                if (reviewerId.HasValue)
                {
                    reviews = await _screeningReviewRepository.GetScreeningReviewsByReviewerAsync(reviewerId.Value);
                }
                else
                {
                    reviews = await _screeningReviewRepository.GetAllScreeningReviewsAsync();
                }

                var pendingCount = reviews.Count(r => r.StatusId == 15); // Pending
                var completedCount = reviews.Count(r => r.StatusId != 15);
                var approvedCount = reviews.Count(r => r.StatusId == 16); // Approved
                var rejectedCount = reviews.Count(r => r.StatusId == 17); // Rejected

                var averageRating = reviews.Any(r => r.Rating.HasValue)
                    ? reviews.Where(r => r.Rating.HasValue).Average(r => r.Rating!.Value)
                    : 0;

                var approvalRate = completedCount > 0
                    ? (decimal)approvedCount / completedCount * 100
                    : 0;

                return new ScreeningStatisticsDto
                {
                    TotalPendingScreenings = pendingCount,
                    TotalCompletedScreenings = completedCount,
                    ApprovedCount = approvedCount,
                    RejectedCount = rejectedCount,
                    AverageRating = averageRating,
                    ApprovalRate = approvalRate
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting screening statistics: {ex.Message}");
            }
        }

        private async Task<ScreeningReviewResponseDto?> MapToResponseDto(Guid screeningReviewId)
        {
            var review = await _screeningReviewRepository.GetScreeningReviewByIdAsync(screeningReviewId);
            if (review == null)
                return null;

            var skillVerifications = await _skillVerificationRepository.GetSkillVerificationsByScreeningAsync(screeningReviewId);



            return new ScreeningReviewResponseDto
            {
                Id = review.Id,
                ApplicationId = review.ApplicationId,
                CandidateName = $"{review.Application?.Candidate?.User?.FirstName} {review.Application?.Candidate?.User?.LastName}",
                CandidateEmail = review.Application?.Candidate?.User?.Email,
                JobTitle = review.Application?.JobPosition?.Title,
                ReviewedBy = review.ReviewedBy,
                ReviewerName = $"{review.Reviewer?.FirstName} {review.Reviewer?.LastName}",
                ReviewDate = review.ReviewDate,
                StatusId = review.StatusId,
                StatusName = review.Status?.StatusName,
                Rating = review.Rating,
                Comments = review.Comments,
                IsRecommendedForInterview = review.IsRecommendedForInterview,
                VerifiedSkills = skillVerifications.Select(sv => new SkillVerificationDto
                {
                    CandidateSkillId = sv.CandidateSkillId,
                    SkillName = sv.CandidateSkill?.Skill?.SkillName,
                    ClaimedYears = sv.CandidateSkill?.YearsOfExperience,
                    VerifiedYears = sv.VerifiedYearsOfExperience,
                    IsVerified = sv.IsVerified,
                    Comments = sv.Comments
                }).ToList(),
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

    }
}