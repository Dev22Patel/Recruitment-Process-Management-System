using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class JobPositionRepository : IJobPositionRepository
    {
        private readonly ApplicationDbContext _context;

        public JobPositionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobPosition> CreateAsync(JobPosition jobPosition)
        {
            _context.JobPositions.Add(jobPosition);
            await _context.SaveChangesAsync();
            return jobPosition;
        }

        public async Task<JobPosition?> GetByIdAsync(Guid id)
        {
            return await _context.JobPositions
                .Include(j => j.Creator)
                .Include(j => j.Status)
                .Include(j => j.JobSkillRequirements)
                    .ThenInclude(jsr => jsr.Skill)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<List<JobPosition>> GetAllAsync()
        {
            return await _context.JobPositions
                .Include(j => j.Creator)
                .Include(j => j.Status)
                .Include(j => j.JobSkillRequirements)
                    .ThenInclude(jsr => jsr.Skill)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<JobPosition>> GetActiveJobsAsync()
        {
            // Assuming StatusId 1 = "Open" or "Active"
            return await _context.JobPositions
                .Include(j => j.Status)
                .Include(j => j.JobSkillRequirements)
                    .ThenInclude(jsr => jsr.Skill)
                .Where(j => j.StatusId == 1) // Active/Open jobs only
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPosition> UpdateAsync(JobPosition jobPosition)
        {
            _context.JobPositions.Update(jobPosition);
            await _context.SaveChangesAsync();
            return jobPosition;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var jobPosition = await _context.JobPositions.FindAsync(id);
            if (jobPosition == null) return false;

            _context.JobPositions.Remove(jobPosition);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.JobPositions.AnyAsync(j => j.Id == id);
        }

        public async Task AddSkillRequirementsAsync(List<JobSkillRequirement> requirements)
        {
            _context.JobSkillRequirements.AddRange(requirements);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSkillRequirementsAsync(Guid jobPositionId)
        {
            var requirements = await _context.JobSkillRequirements
                .Where(jsr => jsr.JobPositionId == jobPositionId)
                .ToListAsync();

            _context.JobSkillRequirements.RemoveRange(requirements);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetJobPositionIdsByReviewerAsync(Guid reviewerId)
        {
            return await _context.JobPositionReviewers
                .Where(jpr => jpr.ReviewerId == reviewerId)
                .Select(jpr => jpr.JobPositionId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<JobSkillRequirement>> GetJobSkillRequirementsAsync(Guid jobPositionId)
        {
            return await _context.JobSkillRequirements
                .Include(jsr => jsr.Skill)
                .Where(jsr => jsr.JobPositionId == jobPositionId)
                .ToListAsync();
        }


        public async Task<JobPosition?> GetJobPositionByIdAsync(Guid id)
        {
            return await _context.JobPositions
                .Include(j => j.Creator)
                .Include(j => j.Status)
                .Include(j => j.JobSkillRequirements)
                    .ThenInclude(jsr => jsr.Skill)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        //public async Task AddReviewersAsync(List<JobPositionReviewer> reviewers)
        //{
        //    _context.JobPositionReviewers.AddRange(reviewers);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task RemoveReviewersAsync(Guid jobPositionId)
        //{
        //    var reviewers = await _context.JobPositionReviewers
        //        .Where(jpr => jpr.JobPositionId == jobPositionId)
        //        .ToListAsync();

        //    _context.JobPositionReviewers.RemoveRange(reviewers);
        //    await _context.SaveChangesAsync();
        //}
    }
}
