using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class ApplicationReviewerRepository : IApplicationReviewerRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationReviewerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationReviewer> CreateAsync(ApplicationReviewer applicationReviewer)
        {
            applicationReviewer.Id = Guid.NewGuid();
            applicationReviewer.AssignedAt = DateTime.UtcNow;

            await _context.ApplicationReviewers.AddAsync(applicationReviewer);
            await _context.SaveChangesAsync();

            return applicationReviewer;
        }

        public async Task<ApplicationReviewer?> GetByApplicationIdAsync(Guid applicationId)
        {
            return await _context.ApplicationReviewers
                .Include(ar => ar.Reviewer)
                .FirstOrDefaultAsync(ar => ar.ApplicationId == applicationId && ar.IsActive);
        }

        public async Task<List<ApplicationReviewer>> GetByReviewerIdAsync(Guid reviewerId)
        {
            return await _context.ApplicationReviewers
                .Include(ar => ar.Application)
                .Where(ar => ar.ReviewerId == reviewerId && ar.IsActive)
                .ToListAsync();
        }

        public async Task<int> GetReviewerWorkloadAsync(Guid reviewerId)
        {
            // Count active applications assigned to this reviewer that are in screening status
            return await _context.ApplicationReviewers
                .Where(ar => ar.ReviewerId == reviewerId
                    && ar.IsActive
                    && ar.Application!.StatusId == 5) // Screening status
                .CountAsync();
        }
    }
}