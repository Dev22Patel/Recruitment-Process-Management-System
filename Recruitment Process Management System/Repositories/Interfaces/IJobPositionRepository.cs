using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IJobPositionRepository
    {
        Task<JobPosition> CreateAsync(JobPosition jobPosition);
        Task<JobPosition?> GetByIdAsync(Guid id);
        Task<List<JobPosition>> GetAllAsync();
        Task<List<JobPosition>> GetActiveJobsAsync(); // For candidates
        Task<JobPosition> UpdateAsync(JobPosition jobPosition);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        // Skill requirements
        Task AddSkillRequirementsAsync(List<JobSkillRequirement> requirements);
        Task RemoveSkillRequirementsAsync(Guid jobPositionId);

        //// Reviewers
        //Task AddReviewersAsync(List<JobPositionReviewer> reviewers);
        //Task RemoveReviewersAsync(Guid jobPositionId);
    }
}
