using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface ICandidateRepository
    {
        Task<Candidate> CreateAsync(Candidate candidate);
        Task<Candidate?> GetByUserIdAsync(Guid userId);
    }
}
