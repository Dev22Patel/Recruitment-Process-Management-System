using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class InterviewService
    {
        private readonly IInterviewRoundRepository _interviewRoundRepository;
        private readonly IInterviewParticipantRepository _participantRepository;
        private readonly IInterviewFeedbackRepository _feedbackRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly EmailService _emailService;
        private readonly IUserRepository _userRepository;

        public InterviewService(
            IInterviewRoundRepository interviewRoundRepository,
            IInterviewParticipantRepository participantRepository,
            IInterviewFeedbackRepository feedbackRepository,
            IApplicationRepository applicationRepository,
            INotificationRepository notificationRepository,
            EmailService emailService,
            IUserRepository userRepository)
        {
            _interviewRoundRepository = interviewRoundRepository;
            _participantRepository = participantRepository;
            _feedbackRepository = feedbackRepository;
            _applicationRepository = applicationRepository;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message, InterviewRoundResponseDto? InterviewRound)> CreateInterviewRoundAsync(
            CreateInterviewRoundDto dto, Guid createdBy)
        {
            // Validate application exists
            var application = await _applicationRepository.GetByIdAsync(dto.ApplicationId);
            if (application == null)
            {
                return (false, "Application not found", null);
            }

            // Check if round number already exists
            var roundExists = await _interviewRoundRepository.ExistsAsync(dto.ApplicationId, dto.RoundNumber);
            if (roundExists)
            {
                return (false, $"Round {dto.RoundNumber} already exists for this application", null);
            }

            // Create interview round
            var interviewRound = new InterviewRound
            {
                ApplicationId = dto.ApplicationId,
                RoundNumber = dto.RoundNumber,
                RoundType = dto.RoundType,
                RoundName = dto.RoundName,
                ScheduledDate = dto.ScheduledDate,
                Duration = dto.Duration,
                MeetingLink = dto.MeetingLink,
                Location = dto.Location,
                StatusId = dto.StatusId,
                CreatedBy = createdBy
            };

            var createdRound = await _interviewRoundRepository.CreateAsync(interviewRound);

            // Add participants if provided
            if (dto.Participants != null && dto.Participants.Any())
            {
                foreach (var participantDto in dto.Participants)
                {
                    var participant = new InterviewParticipant
                    {
                        InterviewRoundId = createdRound.Id,
                        UserId = participantDto.UserId,
                        ParticipantType = participantDto.ParticipantType,
                        AttendanceStatusId = participantDto.AttendanceStatusId
                    };
                    await _participantRepository.CreateAsync(participant);
                }
            }

            // Update application status to "Interview" (StatusId = 6)
            application.StatusId = 6;
            application.StatusReason = $"Interview Round {dto.RoundNumber} scheduled";
            await _applicationRepository.UpdateAsync(application);

            // Send notifications
            await SendInterviewScheduledNotificationsAsync(createdRound, dto.Participants);

            // Get full details
            var response = await GetInterviewRoundByIdAsync(createdRound.Id);

            return (true, "Interview round scheduled successfully", response);
        }

        public async Task<InterviewRoundResponseDto?> GetInterviewRoundByIdAsync(Guid id)
        {
            var interviewRound = await _interviewRoundRepository.GetByIdAsync(id);
            return interviewRound == null ? null : await MapToResponseDto(interviewRound);
        }

        public async Task<List<InterviewRoundResponseDto>> GetAllInterviewRoundsAsync()
        {
            var rounds = await _interviewRoundRepository.GetAllAsync();
            var responseDtos = new List<InterviewRoundResponseDto>();

            foreach (var round in rounds)
            {
                var dto = await MapToResponseDto(round);
                if (dto != null)
                {
                    responseDtos.Add(dto);
                }
            }

            return responseDtos;
        }

        public async Task<List<InterviewRoundResponseDto>> GetInterviewRoundsByApplicationAsync(Guid applicationId)
        {
            var rounds = await _interviewRoundRepository.GetByApplicationIdAsync(applicationId);
            var responseDtos = new List<InterviewRoundResponseDto>();

            foreach (var round in rounds)
            {
                var dto = await MapToResponseDto(round);
                if (dto != null)
                {
                    responseDtos.Add(dto);
                }
            }

            return responseDtos;
        }

        public async Task<List<InterviewerScheduleDto>> GetInterviewerScheduleAsync(Guid userId)
        {
            var participants = await _participantRepository.GetByUserIdAsync(userId);

            return participants.Select(p => new InterviewerScheduleDto
            {
                InterviewRoundId = p.InterviewRoundId,
                ApplicationId = p.InterviewRound?.ApplicationId ?? Guid.Empty,
                CandidateName = p.InterviewRound?.Application?.Candidate?.User != null
                    ? $"{p.InterviewRound.Application.Candidate.User.FirstName} {p.InterviewRound.Application.Candidate.User.LastName}"
                    : null,
                JobTitle = p.InterviewRound?.Application?.JobPosition?.Title,
                RoundNumber = p.InterviewRound?.RoundNumber ?? 0,
                RoundType = p.InterviewRound?.RoundType,
                ScheduledDate = p.InterviewRound?.ScheduledDate,
                Duration = p.InterviewRound?.Duration,
                MeetingLink = p.InterviewRound?.MeetingLink,
                Location = p.InterviewRound?.Location,
                ParticipantType = p.ParticipantType,
                HasSubmittedFeedback = p.InterviewRound != null &&
                    _feedbackRepository.ExistsAsync(p.InterviewRoundId, userId).Result
            }).ToList();
        }

        public async Task<(bool Success, string Message)> UpdateInterviewRoundAsync(UpdateInterviewRoundDto dto)
        {
            var interviewRound = await _interviewRoundRepository.GetByIdAsync(dto.Id);
            if (interviewRound == null)
            {
                return (false, "Interview round not found");
            }

            interviewRound.ScheduledDate = dto.ScheduledDate;
            interviewRound.Duration = dto.Duration;
            interviewRound.MeetingLink = dto.MeetingLink;
            interviewRound.Location = dto.Location;
            interviewRound.StatusId = dto.StatusId;

            await _interviewRoundRepository.UpdateAsync(interviewRound);

            return (true, "Interview round updated successfully");
        }

        public async Task<(bool Success, string Message)> AddParticipantAsync(Guid interviewRoundId, InterviewParticipantDto dto)
        {
            var interviewRound = await _interviewRoundRepository.GetByIdAsync(interviewRoundId);
            if (interviewRound == null)
            {
                return (false, "Interview round not found");
            }

            var exists = await _participantRepository.ExistsAsync(interviewRoundId, dto.UserId);
            if (exists)
            {
                return (false, "Participant already added");
            }

            var participant = new InterviewParticipant
            {
                InterviewRoundId = interviewRoundId,
                UserId = dto.UserId,
                ParticipantType = dto.ParticipantType,
                AttendanceStatusId = dto.AttendanceStatusId
            };

            await _participantRepository.CreateAsync(participant);

            return (true, "Participant added successfully");
        }

        public async Task<(bool Success, string Message, Guid? FeedbackId)> SubmitFeedbackAsync(
            CreateInterviewFeedbackDto dto, Guid interviewerId)
        {
            var interviewRound = await _interviewRoundRepository.GetByIdAsync(dto.InterviewRoundId);
            if (interviewRound == null)
            {
                return (false, "Interview round not found", null);
            }

            // Check if interviewer is a participant
            var isParticipant = await _participantRepository.ExistsAsync(dto.InterviewRoundId, interviewerId);
            if (!isParticipant)
            {
                return (false, "You are not a participant in this interview", null);
            }

            // Check if feedback already exists
            var existingFeedback = await _feedbackRepository.GetByInterviewRoundAndInterviewerAsync(
                dto.InterviewRoundId, interviewerId);

            if (existingFeedback != null)
            {
                return (false, "You have already submitted feedback for this interview", null);
            }

            var feedback = new InterviewFeedback
            {
                InterviewRoundId = dto.InterviewRoundId,
                InterviewerId = interviewerId,
                OverallRating = dto.OverallRating,
                TechnicalRating = dto.TechnicalRating,
                CommunicationRating = dto.CommunicationRating,
                Comments = dto.Comments,
                Recommendation = dto.Recommendation
            };

            var createdFeedback = await _feedbackRepository.CreateAsync(feedback);

            return (true, "Feedback submitted successfully", createdFeedback.Id);
        }

        public async Task<(bool Success, string Message)> UpdateFeedbackAsync(
            UpdateInterviewFeedbackDto dto, Guid interviewerId)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(dto.Id);
            if (feedback == null)
            {
                return (false, "Feedback not found");
            }

            if (feedback.InterviewerId != interviewerId)
            {
                return (false, "You can only update your own feedback");
            }

            feedback.OverallRating = dto.OverallRating;
            feedback.TechnicalRating = dto.TechnicalRating;
            feedback.CommunicationRating = dto.CommunicationRating;
            feedback.Comments = dto.Comments;
            feedback.Recommendation = dto.Recommendation;

            await _feedbackRepository.UpdateAsync(feedback);

            return (true, "Feedback updated successfully");
        }

        public async Task<List<FeedbackSummaryDto>> GetFeedbacksByInterviewRoundAsync(Guid interviewRoundId)
        {
            var feedbacks = await _feedbackRepository.GetByInterviewRoundIdAsync(interviewRoundId);

            return feedbacks.Select(f => new FeedbackSummaryDto
            {
                Id = f.Id,
                InterviewerId = f.InterviewerId,
                InterviewerName = f.Interviewer != null
                    ? $"{f.Interviewer.FirstName} {f.Interviewer.LastName}"
                    : null,
                OverallRating = f.OverallRating,
                TechnicalRating = f.TechnicalRating,
                CommunicationRating = f.CommunicationRating,
                Comments = f.Comments,
                Recommendation = f.Recommendation,
                SubmittedAt = f.SubmittedAt
            }).ToList();
        }

        public async Task<InterviewStatisticsDto> GetInterviewStatisticsAsync()
        {
            var allRounds = await _interviewRoundRepository.GetAllAsync();

            var statistics = new InterviewStatisticsDto
            {
                TotalInterviews = allRounds.Count,
                ScheduledInterviews = allRounds.Count(r => r.StatusId == 9),
                CompletedInterviews = allRounds.Count(r => r.StatusId == 10),
                CancelledInterviews = allRounds.Count(r => r.StatusId == 11),
                InterviewsByType = allRounds.GroupBy(r => r.RoundType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Calculate pending feedbacks
            int pendingFeedbacks = 0;
            decimal totalRating = 0;
            int ratingCount = 0;

            foreach (var round in allRounds)
            {
                var participants = await _participantRepository.GetByInterviewRoundIdAsync(round.Id);
                var feedbacks = await _feedbackRepository.GetByInterviewRoundIdAsync(round.Id);

                pendingFeedbacks += participants.Count - feedbacks.Count;

                foreach (var feedback in feedbacks.Where(f => f.OverallRating.HasValue))
                {
                    totalRating += feedback.OverallRating!.Value;
                    ratingCount++;
                }
            }

            statistics.PendingFeedbacks = pendingFeedbacks;
            statistics.AverageRating = ratingCount > 0 ? totalRating / ratingCount : 0;

            return statistics;
        }

        public async Task<bool> DeleteInterviewRoundAsync(Guid id)
        {
            return await _interviewRoundRepository.DeleteAsync(id);
        }

        // Helper methods
        private async Task<InterviewRoundResponseDto?> MapToResponseDto(InterviewRound round)
        {
            var participants = await _participantRepository.GetByInterviewRoundIdAsync(round.Id);
            var feedbacks = await _feedbackRepository.GetByInterviewRoundIdAsync(round.Id);

            var participantDtos = participants.Select(p => new ParticipantInfoDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : null,
                UserEmail = p.User?.Email,
                ParticipantType = p.ParticipantType,
                AttendanceStatusId = p.AttendanceStatusId,
                AttendanceStatusName = p.AttendanceStatus?.StatusName,
                HasSubmittedFeedback = feedbacks.Any(f => f.InterviewerId == p.UserId)
            }).ToList();

            var feedbackDtos = feedbacks.Select(f => new FeedbackSummaryDto
            {
                Id = f.Id,
                InterviewerId = f.InterviewerId,
                InterviewerName = f.Interviewer != null
                    ? $"{f.Interviewer.FirstName} {f.Interviewer.LastName}"
                    : null,
                OverallRating = f.OverallRating,
                TechnicalRating = f.TechnicalRating,
                CommunicationRating = f.CommunicationRating,
                Comments = f.Comments,
                Recommendation = f.Recommendation,
                SubmittedAt = f.SubmittedAt
            }).ToList();

            var avgRating = feedbacks.Any(f => f.OverallRating.HasValue)
                ? feedbacks.Where(f => f.OverallRating.HasValue).Average(f => f.OverallRating!.Value)
                : (decimal?)null;

            return new InterviewRoundResponseDto
            {
                Id = round.Id,
                ApplicationId = round.ApplicationId,
                CandidateName = round.Application?.Candidate?.User != null
                    ? $"{round.Application.Candidate.User.FirstName} {round.Application.Candidate.User.LastName}"
                    : null,
                CandidateEmail = round.Application?.Candidate?.User?.Email,
                JobTitle = round.Application?.JobPosition?.Title,
                RoundNumber = round.RoundNumber,
                RoundType = round.RoundType,
                RoundName = round.RoundName,
                ScheduledDate = round.ScheduledDate,
                Duration = round.Duration,
                MeetingLink = round.MeetingLink,
                Location = round.Location,
                StatusId = round.StatusId,
                StatusName = round.Status?.StatusName,
                CreatedAt = round.CreatedAt,
                Participants = participantDtos,
                Feedbacks = feedbackDtos,
                AverageRating = avgRating,
                TotalFeedbacksReceived = feedbacks.Count,
                TotalParticipants = participants.Count
            };
        }

        private async Task SendInterviewScheduledNotificationsAsync(
            InterviewRound round, List<InterviewParticipantDto>? participants)
        {
            try
            {
                var application = await _applicationRepository.GetByIdAsync(round.ApplicationId);
                if (application?.Candidate?.UserId == null) return;

                var candidateUser = await _userRepository.GetByIdAsync(application.Candidate.UserId);
                if (candidateUser != null)
                {
                    await _emailService.QueueEmailAsync(
                        candidateUser.Email,
                        "Interview Scheduled",
                        $"Your interview for {application.JobPosition?.Title} has been scheduled for {round.ScheduledDate:MMM dd, yyyy} at {round.ScheduledDate:hh:mm tt}."
                    );
                }

                // Notify participants
                if (participants != null)
                {
                    foreach (var participant in participants)
                    {
                        var user = await _userRepository.GetByIdAsync(participant.UserId);
                        if (user != null)
                        {
                            await _emailService.QueueEmailAsync(
                                user.Email,
                                "Interview Assignment",
                                $"You have been assigned as {participant.ParticipantType} for an interview with {candidateUser?.FirstName} {candidateUser?.LastName} on {round.ScheduledDate:MMM dd, yyyy}."
                            );
                        }
                    }
                }
            }
            catch
            {
                // Log error but don't fail the operation
            }
        }
    }
}