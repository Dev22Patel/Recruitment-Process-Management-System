using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Candidate> CreateAsync(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            return candidate;
        }

        public async Task<Candidate?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Candidates
                .Include(c => c.User)
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Candidate?> GetByIdAsync(Guid candidateId)
        {
            return await _context.Candidates
                .Include(c => c.User)
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.Id == candidateId);
        }

        public async Task<Candidate> UpdateAsync(Candidate candidate)
        {
            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync();
            return candidate;
        }

        public async Task<bool> UpdateCandidateSkillsAsync(Guid candidateId, List<CandidateSkill> skills)
        {
            try
            {
                // Remove existing skills
                var existingSkills = await _context.CandidateSkills
                    .Where(cs => cs.CandidateId == candidateId)
                    .ToListAsync();

                if (existingSkills.Any())
                {
                    _context.CandidateSkills.RemoveRange(existingSkills);
                }

                // Add new skills
                if (skills.Any())
                {
                    await _context.CandidateSkills.AddRangeAsync(skills);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsProfileComplete(Candidate candidate)
        {
            var temp = _context.Candidates.FirstOrDefault(c => c.Id == candidate.Id);
            if (temp == null) return false;
            return temp.IsProfileCompleted;
        }

        public async Task<bool> UpdateProfileCompletionStatusAsync(Guid candidateId, bool isCompleted)
        {
            var candidate = await _context.Candidates.FindAsync(candidateId);
            if (candidate == null) return false;

            candidate.IsProfileCompleted = isCompleted;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
