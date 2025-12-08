using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IInterviewFeedbackRepository
    {
        Task<InterviewFeedback> CreateAsync(InterviewFeedback feedback);
        Task<InterviewFeedback?> GetByIdAsync(Guid id);
        Task<List<InterviewFeedback>> GetByInterviewRoundIdAsync(Guid interviewRoundId);
        Task<List<InterviewFeedback>> GetByInterviewerIdAsync(Guid interviewerId);
        Task<InterviewFeedback?> GetByInterviewRoundAndInterviewerAsync(Guid interviewRoundId, Guid interviewerId);
        Task<InterviewFeedback> UpdateAsync(InterviewFeedback feedback);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid interviewRoundId, Guid interviewerId);
    }
}