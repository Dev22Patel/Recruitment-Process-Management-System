using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IInterviewParticipantRepository
    {
        Task<InterviewParticipant> CreateAsync(InterviewParticipant participant);
        Task<InterviewParticipant?> GetByIdAsync(Guid id);
        Task<List<InterviewParticipant>> GetByInterviewRoundIdAsync(Guid interviewRoundId);
        Task<List<InterviewParticipant>> GetByUserIdAsync(Guid userId);
        Task<InterviewParticipant> UpdateAsync(InterviewParticipant participant);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid interviewRoundId, Guid userId);
    }
}