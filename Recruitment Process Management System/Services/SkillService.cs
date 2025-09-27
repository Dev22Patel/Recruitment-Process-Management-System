using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class SkillService
    {
        private readonly ISkillRepository _skillRepository;

        public SkillService(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
        }

        public async Task<Skill> CreateSkillAsync(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            if (string.IsNullOrWhiteSpace(skill.SkillName)) throw new ArgumentException("Skill name is required.");
            if (string.IsNullOrWhiteSpace(skill.Category)) throw new ArgumentException("Category is required.");

            // Check for duplicate SkillName (additional validation)
            var existingSkill = await _skillRepository.GetByIdAsync(Guid.Empty); // Placeholder for unique check
            if (existingSkill != null) throw new InvalidOperationException("Skill name must be unique.");

            return await _skillRepository.CreateAsync(skill);
        }

        public async Task<Skill?> GetSkillByIdAsync(Guid id)
        {
            return await _skillRepository.GetByIdAsync(id);
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            return await _skillRepository.GetAllAsync();
        }

        public async Task<Skill> UpdateSkillAsync(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            if (string.IsNullOrWhiteSpace(skill.SkillName)) throw new ArgumentException("Skill name is required.");
            if (string.IsNullOrWhiteSpace(skill.Category)) throw new ArgumentException("Category is required.");

            var existingSkill = await _skillRepository.GetByIdAsync(skill.Id);
            if (existingSkill == null) throw new KeyNotFoundException("Skill not found.");

            return await _skillRepository.UpdateAsync(skill);
        }

        public async Task<bool> DeleteSkillAsync(Guid id)
        {
            return await _skillRepository.DeleteAsync(id);
        }

        public async Task<bool> DeactivateSkillAsync(Guid id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null) return false;

            skill.IsActive = false;
            await _skillRepository.UpdateAsync(skill);
            return true;
        }
    }
}
