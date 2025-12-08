using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class InterviewFeedbackRepository : IInterviewFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewFeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewFeedback> CreateAsync(InterviewFeedback feedback)
        {
            feedback.Id = Guid.NewGuid();
            feedback.SubmittedAt = DateTime.UtcNow;

            await _context.InterviewFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(feedback.Id) ?? feedback;
        }

        public async Task<InterviewFeedback?> GetByIdAsync(Guid id)
        {
            return await _context.InterviewFeedbacks
                .Include(f => f.InterviewRound)
                    .ThenInclude(ir => ir!.Application)
                        .ThenInclude(a => a!.Candidate)
                            .ThenInclude(c => c!.User)
                .Include(f => f.Interviewer)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<InterviewFeedback>> GetByInterviewRoundIdAsync(Guid interviewRoundId)
        {
            return await _context.InterviewFeedbacks
                .Include(f => f.Interviewer)
                .Where(f => f.InterviewRoundId == interviewRoundId)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<InterviewFeedback>> GetByInterviewerIdAsync(Guid interviewerId)
        {
            return await _context.InterviewFeedbacks
                .Include(f => f.InterviewRound)
                    .ThenInclude(ir => ir!.Application)
                        .ThenInclude(a => a!.Candidate)
                            .ThenInclude(c => c!.User)
                .Include(f => f.InterviewRound)
                    .ThenInclude(ir => ir!.Application)
                        .ThenInclude(a => a!.JobPosition)
                .Where(f => f.InterviewerId == interviewerId)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<InterviewFeedback?> GetByInterviewRoundAndInterviewerAsync(Guid interviewRoundId, Guid interviewerId)
        {
            return await _context.InterviewFeedbacks
                .Include(f => f.InterviewRound)
                .Include(f => f.Interviewer)
                .FirstOrDefaultAsync(f => f.InterviewRoundId == interviewRoundId && f.InterviewerId == interviewerId);
        }

        public async Task<InterviewFeedback> UpdateAsync(InterviewFeedback feedback)
        {
            _context.InterviewFeedbacks.Update(feedback);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(feedback.Id) ?? feedback;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var feedback = await _context.InterviewFeedbacks.FindAsync(id);
            if (feedback == null) return false;

            _context.InterviewFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid interviewRoundId, Guid interviewerId)
        {
            return await _context.InterviewFeedbacks
                .AnyAsync(f => f.InterviewRoundId == interviewRoundId && f.InterviewerId == interviewerId);
        }
    }
}