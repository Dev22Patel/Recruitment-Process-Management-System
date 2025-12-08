using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IInterviewRoundRepository
    {
        Task<InterviewRound> CreateAsync(InterviewRound interviewRound);
        Task<InterviewRound?> GetByIdAsync(Guid id);
        Task<List<InterviewRound>> GetAllAsync();
        Task<List<InterviewRound>> GetByApplicationIdAsync(Guid applicationId);
        Task<List<InterviewRound>> GetByInterviewerIdAsync(Guid interviewerId);
        Task<List<InterviewRound>> GetUpcomingInterviewsAsync(DateTime fromDate);
        Task<InterviewRound> UpdateAsync(InterviewRound interviewRound);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid applicationId, int roundNumber);
        Task<int> GetRoundCountByApplicationAsync(Guid applicationId);
    }
}