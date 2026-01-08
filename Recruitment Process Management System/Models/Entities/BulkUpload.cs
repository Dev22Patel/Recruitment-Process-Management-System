using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_Process_Management_System.Models.Entities
{
    [Table("BulkUploads")]
    public class BulkUpload
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; }

        [MaxLength(50)]
        public string UploadType { get; set; } // "Excel" or "CV_Zip"

        public int TotalRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }

        [Required]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorLog { get; set; }

        [Required]
        public Guid UploadedBy { get; set; }

        [ForeignKey("UploadedBy")]
        public User UploadedByUser { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}