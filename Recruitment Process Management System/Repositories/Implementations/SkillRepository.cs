using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ApplicationDbContext _context;

        public SkillRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Skill> CreateAsync(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            // Ensure Id is generated if not provided
            if (skill.Id == Guid.Empty)
            {
                skill.Id = Guid.NewGuid();
            }

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<Skill?> GetByIdAsync(Guid id)
        {
            return await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Skill>> GetAllAsync()
        {
            return await _context.Skills.ToListAsync();
        }

        public async Task<Skill> UpdateAsync(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            var existingSkill = await _context.Skills.FindAsync(skill.Id);
            if (existingSkill == null) throw new KeyNotFoundException("Skill not found.");

            // Update properties
            existingSkill.SkillName = skill.SkillName;
            existingSkill.Category = skill.Category;
            existingSkill.IsActive = skill.IsActive;

            _context.Skills.Update(existingSkill);
            await _context.SaveChangesAsync();
            return existingSkill;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Skill?> GetByNameAsync(string name)
        {
            return await _context.Skills.FirstOrDefaultAsync(s => s.SkillName.ToLower() == name.ToLower());
        }
    }
}
