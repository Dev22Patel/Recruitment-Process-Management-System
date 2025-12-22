using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IScreeningReviewRepository
    {
        Task<ScreeningReview> CreateScreeningReviewAsync(ScreeningReview screeningReview);
        Task<ScreeningReview?> GetScreeningReviewByIdAsync(Guid id);
        Task<List<ScreeningReview>> GetScreeningReviewsByApplicationAsync(Guid applicationId);
        Task<List<ScreeningReview>> GetScreeningReviewsByReviewerAsync(Guid reviewerId);
        Task<List<ScreeningReview>> GetPendingScreeningsForReviewerAsync(Guid reviewerId);
        Task<ScreeningReview> UpdateScreeningReviewAsync(ScreeningReview screeningReview);
        Task<List<ScreeningReview>> GetAllScreeningReviewsAsync();
        Task<ScreeningReview?> GetScreeningReviewByApplicationIdAsync(Guid applicationId);
        Task<bool> DeleteScreeningReviewAsync(Guid id);
    }
}