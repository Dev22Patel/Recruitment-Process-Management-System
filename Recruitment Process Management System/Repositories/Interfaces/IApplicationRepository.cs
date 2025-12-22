using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IApplicationRepository
    {
        Task<Application> CreateAsync(Application application);
        Task<Application?> GetByIdAsync(Guid id);
        Task<List<Application>> GetAllAsync();
        Task<List<Application>> GetByCandidateIdAsync(Guid candidateId);
        Task<List<Application>> GetByJobPositionIdAsync(Guid jobPositionId);
        Task<Application?> GetByJobAndCandidateAsync(Guid jobPositionId, Guid candidateId);
        Task<Application> UpdateAsync(Application application);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid candidateId, Guid jobPositionId);
        Task<List<Application>> GetApplicationsByStatusAsync(int statusId);
        Task<int> GetApplicationCountByJobAsync(Guid jobPositionId);
        Task<int> GetApplicationCountByCandidateAsync(Guid candidateId);

        Task<List<Application>> GetApplicationsByStatusAndJobsAsync(int statusId, List<Guid> jobPositionIds);

    }
}