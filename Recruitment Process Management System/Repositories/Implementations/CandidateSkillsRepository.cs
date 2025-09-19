using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class CandidateSkillsRepository : ICandidateSkillsRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateSkillsRepository(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
