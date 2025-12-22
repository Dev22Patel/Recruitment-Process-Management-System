using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class JobPositionReviewerRepository : IJobPositionReviewerRepository
    {
        private readonly ApplicationDbContext _context;

        public JobPositionReviewerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobPositionReviewer> AssignReviewerToJobAsync(JobPositionReviewer jobPositionReviewer)
        {
            _context.JobPositionReviewers.Add(jobPositionReviewer);
            await _context.SaveChangesAsync();
            return jobPositionReviewer;
        }

        public async Task<bool> RemoveReviewerFromJobAsync(Guid jobPositionReviewerId)
        {
            var jobPositionReviewer = await _context.JobPositionReviewers.FindAsync(jobPositionReviewerId);
            if (jobPositionReviewer == null) return false;

            _context.JobPositionReviewers.Remove(jobPositionReviewer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsReviewerAssignedToJobAsync(Guid jobPositionId, Guid reviewerId)
        {
            return await _context.JobPositionReviewers
                .AnyAsync(jpr => jpr.JobPositionId == jobPositionId
                    && jpr.ReviewerId == reviewerId
                    && jpr.IsActive);
        }

        public async Task<List<JobPositionReviewer>> GetReviewersByJobPositionAsync(Guid jobPositionId)
        {
            return await _context.JobPositionReviewers
                .Include(jpr => jpr.JobPosition)
                .Include(jpr => jpr.Reviewer)
                .Include(jpr => jpr.Assigner)
                .Where(jpr => jpr.JobPositionId == jobPositionId && jpr.IsActive)
                .OrderByDescending(jpr => jpr.AssignedAt)
                .ToListAsync();
        }

        public async Task<List<JobPositionReviewer>> GetJobPositionsByReviewerAsync(Guid reviewerId)
        {
            return await _context.JobPositionReviewers
                .Include(jpr => jpr.JobPosition)
                    .ThenInclude(jp => jp.Status)
                .Include(jpr => jpr.Reviewer)
                .Include(jpr => jpr.Assigner)
                .Where(jpr => jpr.ReviewerId == reviewerId && jpr.IsActive)
                .OrderByDescending(jpr => jpr.AssignedAt)
                .ToListAsync();
        }

        public async Task<JobPositionReviewer?> GetByIdAsync(Guid id)
        {
            return await _context.JobPositionReviewers
                .Include(jpr => jpr.JobPosition)
                .Include(jpr => jpr.Reviewer)
                .Include(jpr => jpr.Assigner)
                .FirstOrDefaultAsync(jpr => jpr.Id == id);
        }

        public async Task<bool> DeactivateReviewerAssignmentAsync(Guid jobPositionReviewerId)
        {
            var jobPositionReviewer = await _context.JobPositionReviewers.FindAsync(jobPositionReviewerId);
            if (jobPositionReviewer == null) return false;

            jobPositionReviewer.IsActive = false;
            jobPositionReviewer.RemovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}