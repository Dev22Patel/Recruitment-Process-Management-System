using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs
{
    // DTO for creating an interview round
    public class CreateInterviewRoundDto
    {
        [Required(ErrorMessage = "Application ID is required")]
        public Guid ApplicationId { get; set; }

        [Required(ErrorMessage = "Round number is required")]
        [Range(1, 10, ErrorMessage = "Round number must be between 1 and 10")]
        public int RoundNumber { get; set; }

        [Required(ErrorMessage = "Round type is required")]
        [MaxLength(50)]
        public string RoundType { get; set; } = string.Empty; // Technical, HR, Panel

        [MaxLength(100)]
        public string? RoundName { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes")]
        public int? Duration { get; set; }

        [MaxLength(500)]
        [Url(ErrorMessage = "Meeting link must be a valid URL")]
        public string? MeetingLink { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public int StatusId { get; set; }

        public List<InterviewParticipantDto>? Participants { get; set; }
    }

    // DTO for interview participant
    public class InterviewParticipantDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ParticipantType { get; set; } = string.Empty; // Primary_Interviewer, Co_Interviewer

        [Required]
        public int AttendanceStatusId { get; set; }
    }

    // DTO for updating interview round
    public class UpdateInterviewRoundDto
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [Range(15, 480)]
        public int? Duration { get; set; }

        [MaxLength(500)]
        [Url]
        public string? MeetingLink { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public int StatusId { get; set; }
    }

    // DTO for interview round response
    public class InterviewRoundResponseDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? JobTitle { get; set; }
        public int RoundNumber { get; set; }
        public string? RoundType { get; set; }
        public string? RoundName { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int? Duration { get; set; }
        public string? MeetingLink { get; set; }
        public string? Location { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ParticipantInfoDto>? Participants { get; set; }
        public List<FeedbackSummaryDto>? Feedbacks { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalFeedbacksReceived { get; set; }
        public int TotalParticipants { get; set; }
    }

    // DTO for participant information
    public class ParticipantInfoDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? ParticipantType { get; set; }
        public int AttendanceStatusId { get; set; }
        public string? AttendanceStatusName { get; set; }
        public bool HasSubmittedFeedback { get; set; }
    }

    // DTO for feedback summary
    public class FeedbackSummaryDto
    {
        public Guid Id { get; set; }
        public Guid InterviewerId { get; set; }
        public string? InterviewerName { get; set; }
        public decimal? OverallRating { get; set; }
        public decimal? TechnicalRating { get; set; }
        public decimal? CommunicationRating { get; set; }
        public string? Comments { get; set; }
        public string? Recommendation { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    // DTO for creating interview feedback
    public class CreateInterviewFeedbackDto
    {
        [Required]
        public Guid InterviewRoundId { get; set; }

        [Range(1, 5, ErrorMessage = "Overall rating must be between 1 and 5")]
        public decimal? OverallRating { get; set; }

        [Range(1, 5, ErrorMessage = "Technical rating must be between 1 and 5")]
        public decimal? TechnicalRating { get; set; }

        [Range(1, 5, ErrorMessage = "Communication rating must be between 1 and 5")]
        public decimal? CommunicationRating { get; set; }

        public string? Comments { get; set; }

        public string? Recommendation { get; set; }
    }

    // DTO for updating interview feedback
    public class UpdateInterviewFeedbackDto
    {
        [Required]
        public Guid Id { get; set; }

        [Range(1, 5)]
        public decimal? OverallRating { get; set; }

        [Range(1, 5)]
        public decimal? TechnicalRating { get; set; }

        [Range(1, 5)]
        public decimal? CommunicationRating { get; set; }

        public string? Comments { get; set; }

        public string? Recommendation { get; set; }
    }

    // DTO for interview statistics
    public class InterviewStatisticsDto
    {
        public int TotalInterviews { get; set; }
        public int ScheduledInterviews { get; set; }
        public int CompletedInterviews { get; set; }
        public int CancelledInterviews { get; set; }
        public int PendingFeedbacks { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<string, int>? InterviewsByType { get; set; }
    }

    // DTO for interviewer's schedule
    public class InterviewerScheduleDto
    {
        public Guid InterviewRoundId { get; set; }
        public Guid ApplicationId { get; set; }
        public string? CandidateName { get; set; }
        public string? JobTitle { get; set; }
        public int RoundNumber { get; set; }
        public string? RoundType { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int? Duration { get; set; }
        public string? MeetingLink { get; set; }
        public string? Location { get; set; }
        public string? ParticipantType { get; set; }
        public bool HasSubmittedFeedback { get; set; }
    }
}