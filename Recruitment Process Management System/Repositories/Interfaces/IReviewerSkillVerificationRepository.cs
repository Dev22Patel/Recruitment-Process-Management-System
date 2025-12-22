using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IReviewerSkillVerificationRepository
    {
        Task<ReviewerSkillVerification> CreateSkillVerificationAsync(ReviewerSkillVerification skillVerification);
        Task<List<ReviewerSkillVerification>> BulkCreateSkillVerificationsAsync(List<ReviewerSkillVerification> skillVerifications);
        Task<ReviewerSkillVerification?> GetSkillVerificationAsync(Guid screeningReviewId, Guid candidateSkillId);
        Task<List<ReviewerSkillVerification>> GetSkillVerificationsByScreeningAsync(Guid screeningReviewId);
        Task<ReviewerSkillVerification> UpdateSkillVerificationAsync(ReviewerSkillVerification skillVerification);
        Task<bool> DeleteSkillVerificationAsync(Guid id);
        Task<List<ReviewerSkillVerification>> GetSkillVerificationsByReviewerAsync(Guid reviewerId);
    }
}