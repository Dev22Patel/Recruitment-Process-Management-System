using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface ICandidateRepository
    {
        Task<Candidate> CreateAsync(Candidate candidate);
        Task<Candidate?> GetByUserIdAsync(Guid userId);
        Task<Candidate?> GetByIdAsync(Guid candidateId);
        Task<Candidate> UpdateAsync(Candidate candidate);
        Task<bool> UpdateCandidateSkillsAsync(Guid candidateId, List<CandidateSkill> skills);
        bool IsProfileComplete(Candidate candidate);
        Task<bool> UpdateProfileCompletionStatusAsync(Guid candidateId, bool isCompleted);

        Task<List<CandidateSkill>> GetSkillsByIdAsync(Guid candidateId);

        Task<Candidate?> GetCandidateByApplicationIdAsync(Guid applicationId);

    }
}
