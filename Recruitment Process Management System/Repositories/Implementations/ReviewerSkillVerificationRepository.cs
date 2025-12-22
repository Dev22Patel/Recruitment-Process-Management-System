using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class ReviewerSkillVerificationRepository : IReviewerSkillVerificationRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewerSkillVerificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewerSkillVerification> CreateSkillVerificationAsync(ReviewerSkillVerification skillVerification)
        {
            _context.ReviewerSkillVerifications.Add(skillVerification);
            await _context.SaveChangesAsync();
            return skillVerification;
        }

        public async Task<List<ReviewerSkillVerification>> BulkCreateSkillVerificationsAsync(List<ReviewerSkillVerification> skillVerifications)
        {
            _context.ReviewerSkillVerifications.AddRange(skillVerifications);
            await _context.SaveChangesAsync();
            return skillVerifications;
        }

        public async Task<ReviewerSkillVerification?> GetSkillVerificationAsync(Guid screeningReviewId, Guid candidateSkillId)
        {
            return await _context.ReviewerSkillVerifications
                .Include(sv => sv.ScreeningReview)
                .Include(sv => sv.CandidateSkill)
                    .ThenInclude(cs => cs.Skill)
                .Include(sv => sv.Verifier)
                .FirstOrDefaultAsync(sv => sv.ScreeningReviewId == screeningReviewId
                    && sv.CandidateSkillId == candidateSkillId);
        }

        public async Task<List<ReviewerSkillVerification>> GetSkillVerificationsByScreeningAsync(Guid screeningReviewId)
        {
            return await _context.ReviewerSkillVerifications
                .Include(sv => sv.CandidateSkill)
                    .ThenInclude(cs => cs.Skill)
                .Include(sv => sv.Verifier)
                .Where(sv => sv.ScreeningReviewId == screeningReviewId)
                .OrderBy(sv => sv.VerifiedAt)
                .ToListAsync();
        }

        public async Task<ReviewerSkillVerification> UpdateSkillVerificationAsync(ReviewerSkillVerification skillVerification)
        {
            _context.ReviewerSkillVerifications.Update(skillVerification);
            await _context.SaveChangesAsync();
            return skillVerification;
        }

        public async Task<bool> DeleteSkillVerificationAsync(Guid id)
        {
            var skillVerification = await _context.ReviewerSkillVerifications.FindAsync(id);
            if (skillVerification == null) return false;

            _context.ReviewerSkillVerifications.Remove(skillVerification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReviewerSkillVerification>> GetSkillVerificationsByReviewerAsync(Guid reviewerId)
        {
            return await _context.ReviewerSkillVerifications
                .Include(sv => sv.ScreeningReview)
                .Include(sv => sv.CandidateSkill)
                    .ThenInclude(cs => cs.Skill)
                .Where(sv => sv.VerifiedBy == reviewerId)
                .OrderByDescending(sv => sv.VerifiedAt)
                .ToListAsync();
        }
    }
}