using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IJobPositionReviewerRepository
    {
        Task<JobPositionReviewer> AssignReviewerToJobAsync(JobPositionReviewer jobPositionReviewer);
        Task<bool> RemoveReviewerFromJobAsync(Guid jobPositionReviewerId);
        Task<bool> IsReviewerAssignedToJobAsync(Guid jobPositionId, Guid reviewerId);
        Task<List<JobPositionReviewer>> GetReviewersByJobPositionAsync(Guid jobPositionId);
        Task<List<JobPositionReviewer>> GetJobPositionsByReviewerAsync(Guid reviewerId);
        Task<JobPositionReviewer?> GetByIdAsync(Guid id);
        Task<bool> DeactivateReviewerAssignmentAsync(Guid jobPositionReviewerId);
    }
}