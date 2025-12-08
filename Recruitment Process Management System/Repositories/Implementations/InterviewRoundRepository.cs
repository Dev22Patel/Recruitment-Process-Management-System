using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class InterviewRoundRepository : IInterviewRoundRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewRoundRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewRound> CreateAsync(InterviewRound interviewRound)
        {
            interviewRound.Id = Guid.NewGuid();
            interviewRound.CreatedAt = DateTime.UtcNow;

            await _context.InterviewRounds.AddAsync(interviewRound);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(interviewRound.Id) ?? interviewRound;
        }

        public async Task<InterviewRound?> GetByIdAsync(Guid id)
        {
            return await _context.InterviewRounds
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(ir => ir.Status)
                .Include(ir => ir.Creator)
                .Include(ir => ir.InterviewParticipants)
                    .ThenInclude(ip => ip.User)
                .Include(ir => ir.InterviewFeedbacks)
                    .ThenInclude(f => f.Interviewer)
                .FirstOrDefaultAsync(ir => ir.Id == id);
        }

        public async Task<List<InterviewRound>> GetAllAsync()
        {
            return await _context.InterviewRounds
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(ir => ir.Status)
                .Include(ir => ir.InterviewParticipants)
                    .ThenInclude(ip => ip.User)
                .OrderByDescending(ir => ir.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<InterviewRound>> GetByApplicationIdAsync(Guid applicationId)
        {
            return await _context.InterviewRounds
                .Include(ir => ir.Status)
                .Include(ir => ir.InterviewParticipants)
                    .ThenInclude(ip => ip.User)
                .Include(ir => ir.InterviewFeedbacks)
                .Where(ir => ir.ApplicationId == applicationId)
                .OrderBy(ir => ir.RoundNumber)
                .ToListAsync();
        }

        public async Task<List<InterviewRound>> GetByInterviewerIdAsync(Guid interviewerId)
        {
            return await _context.InterviewRounds
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(ir => ir.Status)
                .Include(ir => ir.InterviewParticipants)
                    .ThenInclude(ip => ip.User)
                .Where(ir => ir.InterviewParticipants.Any(ip => ip.UserId == interviewerId))
                .OrderByDescending(ir => ir.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<InterviewRound>> GetUpcomingInterviewsAsync(DateTime fromDate)
        {
            return await _context.InterviewRounds
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.Candidate)
                        .ThenInclude(c => c!.User)
                .Include(ir => ir.Application)
                    .ThenInclude(a => a!.JobPosition)
                .Include(ir => ir.Status)
                .Include(ir => ir.InterviewParticipants)
                    .ThenInclude(ip => ip.User)
                .Where(ir => ir.ScheduledDate >= fromDate)
                .OrderBy(ir => ir.ScheduledDate)
                .ToListAsync();
        }

        public async Task<InterviewRound> UpdateAsync(InterviewRound interviewRound)
        {
            _context.InterviewRounds.Update(interviewRound);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(interviewRound.Id) ?? interviewRound;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var interviewRound = await _context.InterviewRounds.FindAsync(id);
            if (interviewRound == null) return false;

            _context.InterviewRounds.Remove(interviewRound);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid applicationId, int roundNumber)
        {
            return await _context.InterviewRounds
                .AnyAsync(ir => ir.ApplicationId == applicationId && ir.RoundNumber == roundNumber);
        }

        public async Task<int> GetRoundCountByApplicationAsync(Guid applicationId)
        {
            return await _context.InterviewRounds
                .CountAsync(ir => ir.ApplicationId == applicationId);
        }
    }
}