using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface ISkillRepository
    {
        Task<Skill> CreateAsync(Skill skill);
        Task<Skill?> GetByIdAsync(Guid id);
        Task<List<Skill>> GetAllAsync();
        Task<Skill> UpdateAsync(Skill skill);
        Task<bool> DeleteAsync(Guid id);
    }
}
