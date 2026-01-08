using Microsoft.EntityFrameworkCore;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Repositories.Implementations
{
    public class BulkUploadRepository : IBulkUploadRepository
    {
        private readonly ApplicationDbContext _context;

        public BulkUploadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BulkUpload> CreateAsync(BulkUpload bulkUpload)
        {
            await _context.BulkUploads.AddAsync(bulkUpload);
            await _context.SaveChangesAsync();
            return bulkUpload;
        }

        public async Task<BulkUpload?> GetByIdAsync(Guid id)
        {
            return await _context.BulkUploads
                .Include(b => b.Status)
                .Include(b => b.UploadedByUser)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BulkUpload>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            return await _context.BulkUploads
                .Include(b => b.Status)
                .Include(b => b.UploadedByUser)
                .OrderByDescending(b => b.UploadedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<BulkUpload> UpdateAsync(BulkUpload bulkUpload)
        {
            _context.BulkUploads.Update(bulkUpload);
            await _context.SaveChangesAsync();
            return bulkUpload;
        }

        public async Task<List<BulkUpload>> GetByUploaderIdAsync(Guid uploaderId)
        {
            return await _context.BulkUploads
                .Include(b => b.Status)
                .Where(b => b.UploadedBy == uploaderId)
                .OrderByDescending(b => b.UploadedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.BulkUploads.CountAsync();
        }
    }
}
