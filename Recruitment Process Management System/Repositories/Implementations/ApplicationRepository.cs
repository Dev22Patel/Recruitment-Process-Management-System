using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Application> CreateAsync(Application application)
        {
            application.Id = Guid.NewGuid();
            application.ApplicationDate = DateTime.UtcNow;
            application.CreatedAt = DateTime.UtcNow;

            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(application.Id) ?? application;
        }

        public async Task<Application?> GetByIdAsync(Guid id)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c!.User)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c!.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                .Include(a => a.JobPosition)
                    .ThenInclude(jp => jp!.JobSkillRequirements)
                        .ThenInclude(jsr => jsr.Skill)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Application>> GetAllAsync()
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c!.User)
                .Include(a => a.JobPosition)
                .Include(a => a.Status)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
        }

        public async Task<List<Application>> GetByCandidateIdAsync(Guid candidateId)
        {
            return await _context.Applications
                .Include(a => a.JobPosition)
                    .ThenInclude(jp => jp!.JobSkillRequirements)
                        .ThenInclude(jsr => jsr.Skill)
                .Include(a => a.Status)
                .Where(a => a.CandidateId == candidateId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
        }

        public async Task<List<Application>> GetByJobPositionIdAsync(Guid jobPositionId)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c!.User)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c!.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                .Include(a => a.Status)
                .Where(a => a.JobPositionId == jobPositionId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
        }

        public async Task<Application?> GetByJobAndCandidateAsync(Guid jobPositionId, Guid candidateId)
        {
            return await _context.Applications
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.JobPositionId == jobPositionId && a.CandidateId == candidateId);
        }

        public async Task<Application> UpdateAsync(Application application)
        {
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(application.Id) ?? application;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return false;

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid candidateId, Guid jobPositionId)
        {
            return await _context.Applications
                .AnyAsync(a => a.CandidateId == candidateId && a.JobPositionId == jobPositionId);
        }

        public async Task<List<Application>> GetApplicationsByStatusAsync(int statusId)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c!.User)
                .Include(a => a.JobPosition)
                .Include(a => a.Status)
                .Where(a => a.StatusId == statusId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
        }

        public async Task<int> GetApplicationCountByJobAsync(Guid jobPositionId)
        {
            return await _context.Applications
                .CountAsync(a => a.JobPositionId == jobPositionId);
        }

        public async Task<int> GetApplicationCountByCandidateAsync(Guid candidateId)
        {
            return await _context.Applications
                .CountAsync(a => a.CandidateId == candidateId);
        }

        public async Task<List<Application>> GetApplicationsByStatusAndJobsAsync(int statusId, List<Guid> jobPositionIds)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.User)
                .Include(a => a.JobPosition)
                .Include(a => a.Status)
                .Where(a => a.StatusId == statusId && jobPositionIds.Contains(a.JobPositionId))
                .OrderBy(a => a.ApplicationDate)
                .ToListAsync();
        }
    }
}