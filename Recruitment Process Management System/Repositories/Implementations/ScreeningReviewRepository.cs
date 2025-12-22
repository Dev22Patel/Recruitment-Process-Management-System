using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class ScreeningReviewRepository : IScreeningReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ScreeningReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ScreeningReview> CreateScreeningReviewAsync(ScreeningReview screeningReview)
        {
            await _context.ScreeningReviews.AddAsync(screeningReview);
            await _context.SaveChangesAsync();
            return screeningReview;
        }

        public async Task<ScreeningReview?> GetScreeningReviewByIdAsync(Guid id)
        {
            return await _context.ScreeningReviews
                .Include(sr => sr.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(sr => sr.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(sr => sr.Reviewer)
                .Include(sr => sr.Status)
                .Include(sr => sr.SkillVerifications)
                    .ThenInclude(sv => sv.CandidateSkill)
                        .ThenInclude(cs => cs!.Skill)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<List<ScreeningReview>> GetScreeningReviewsByApplicationAsync(Guid applicationId)
        {
            return await _context.ScreeningReviews
                .Include(sr => sr.Reviewer)
                .Include(sr => sr.Status)
                .Include(sr => sr.SkillVerifications)
                .Where(sr => sr.ApplicationId == applicationId)
                .OrderByDescending(sr => sr.ReviewDate)
                .ToListAsync();
        }

        public async Task<List<ScreeningReview>> GetScreeningReviewsByReviewerAsync(Guid reviewerId)
        {
            return await _context.ScreeningReviews
                .Include(sr => sr.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(sr => sr.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(sr => sr.Status)
                .Where(sr => sr.ReviewedBy == reviewerId)
                .OrderByDescending(sr => sr.ReviewDate)
                .ToListAsync();
        }

        public async Task<List<ScreeningReview>> GetPendingScreeningsForReviewerAsync(Guid reviewerId)
        {
            // Get job positions assigned to this reviewer
            var assignedJobIds = await _context.JobPositionReviewers
                .Where(jpr => jpr.ReviewerId == reviewerId && jpr.IsActive)
                .Select(jpr => jpr.JobPositionId)
                .ToListAsync();

            // Get applications for these jobs with "Screening" status (StatusId = 5)
            var pendingApplicationIds = await _context.Applications
                .Where(a => assignedJobIds.Contains(a.JobPositionId) && a.StatusId == 5)
                .Select(a => a.Id)
                .ToListAsync();

            // Get applications that don't have a screening review yet
            var alreadyScreenedAppIds = await _context.ScreeningReviews
                .Where(sr => pendingApplicationIds.Contains(sr.ApplicationId))
                .Select(sr => sr.ApplicationId)
                .ToListAsync();

            var pendingAppIds = pendingApplicationIds.Except(alreadyScreenedAppIds).ToList();

            // Return empty list since these are applications without reviews yet
            // This method is used to check which applications need screening
            return new List<ScreeningReview>();
        }

        public async Task<ScreeningReview> UpdateScreeningReviewAsync(ScreeningReview screeningReview)
        {
            screeningReview.UpdatedAt = DateTime.UtcNow;
            _context.ScreeningReviews.Update(screeningReview);
            await _context.SaveChangesAsync();
            return screeningReview;
        }


        public async Task<List<ScreeningReview>> GetAllScreeningReviewsAsync()
        {
            return await _context.ScreeningReviews
                .Include(sr => sr.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(sr => sr.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(sr => sr.Reviewer)
                .Include(sr => sr.Status)
                .OrderByDescending(sr => sr.ReviewDate)
                .ToListAsync();
        }

        public async Task<ScreeningReview?> GetScreeningReviewByApplicationIdAsync(Guid applicationId)
        {
            return await _context.ScreeningReviews
                .Include(sr => sr.Reviewer)
                .Include(sr => sr.Status)
                .Include(sr => sr.SkillVerifications)
                .FirstOrDefaultAsync(sr => sr.ApplicationId == applicationId);
        }

        public async Task<bool> DeleteScreeningReviewAsync(Guid id)
        {
            var screeningReview = await _context.ScreeningReviews.FindAsync(id);
            if (screeningReview == null)
                return false;

            _context.ScreeningReviews.Remove(screeningReview);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}