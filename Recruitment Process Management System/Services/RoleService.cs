using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;


namespace Recruitment_Process_Management_System.Services
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<Role>> GetAllActiveRolesAsync()
        {
            return await _roleRepository.GetAllActiveRolesAsync();
        }
    }
}