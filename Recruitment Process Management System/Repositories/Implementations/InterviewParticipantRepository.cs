using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class InterviewParticipantRepository : IInterviewParticipantRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewParticipantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewParticipant> CreateAsync(InterviewParticipant participant)
        {
            participant.Id = Guid.NewGuid();
            participant.CreatedAt = DateTime.UtcNow;

            await _context.InterviewParticipants.AddAsync(participant);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(participant.Id) ?? participant;
        }

        public async Task<InterviewParticipant?> GetByIdAsync(Guid id)
        {
            return await _context.InterviewParticipants
                .Include(ip => ip.InterviewRound)
                .Include(ip => ip.User)
                .Include(ip => ip.AttendanceStatus)
                .FirstOrDefaultAsync(ip => ip.Id == id);
        }

        public async Task<List<InterviewParticipant>> GetByInterviewRoundIdAsync(Guid interviewRoundId)
        {
            return await _context.InterviewParticipants
                .Include(ip => ip.User)
                .Include(ip => ip.AttendanceStatus)
                .Where(ip => ip.InterviewRoundId == interviewRoundId)
                .ToListAsync();
        }

        public async Task<List<InterviewParticipant>> GetByUserIdAsync(Guid userId)
        {
            return await _context.InterviewParticipants
                .Include(ip => ip.InterviewRound)
                    .ThenInclude(ir => ir!.Application)
                        .ThenInclude(a => a!.Candidate)
                            .ThenInclude(c => c!.User)
                .Include(ip => ip.InterviewRound)
                    .ThenInclude(ir => ir!.Application)
                        .ThenInclude(a => a!.JobPosition)
                .Include(ip => ip.AttendanceStatus)
                .Where(ip => ip.UserId == userId)
                .OrderByDescending(ip => ip.InterviewRound!.ScheduledDate)
                .ToListAsync();
        }

        public async Task<InterviewParticipant> UpdateAsync(InterviewParticipant participant)
        {
            _context.InterviewParticipants.Update(participant);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(participant.Id) ?? participant;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var participant = await _context.InterviewParticipants.FindAsync(id);
            if (participant == null) return false;

            _context.InterviewParticipants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid interviewRoundId, Guid userId)
        {
            return await _context.InterviewParticipants
                .AnyAsync(ip => ip.InterviewRoundId == interviewRoundId && ip.UserId == userId);
        }
    }
}