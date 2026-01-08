using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Repositories.Interfaces
{
    public interface IBulkUploadRepository
    {
        Task<BulkUpload> CreateAsync(BulkUpload bulkUpload);
        Task<BulkUpload?> GetByIdAsync(Guid id);
        Task<List<BulkUpload>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
        Task<BulkUpload> UpdateAsync(BulkUpload bulkUpload);
        Task<List<BulkUpload>> GetByUploaderIdAsync(Guid uploaderId);
        Task<int> GetTotalCountAsync();
    }
}
