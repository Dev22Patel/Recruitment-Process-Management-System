using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IApplicationReviewerRepository
    {
        Task<ApplicationReviewer> CreateAsync(ApplicationReviewer applicationReviewer);
        Task<ApplicationReviewer?> GetByApplicationIdAsync(Guid applicationId);
        Task<List<ApplicationReviewer>> GetByReviewerIdAsync(Guid reviewerId);
        Task<int> GetReviewerWorkloadAsync(Guid reviewerId);
    }
}