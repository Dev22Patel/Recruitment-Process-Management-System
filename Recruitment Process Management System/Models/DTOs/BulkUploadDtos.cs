using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs
{
    // Request DTO for bulk upload
    public class BulkUploadRequest
    {
        [Required(ErrorMessage = "Excel file is required")]
        public IFormFile ExcelFile { get; set; }

        public Guid? RecruitmentEventId { get; set; } // Optional: Link to campus event
    }

    // Response DTO after initiating upload
    public class BulkUploadResponse
    {
        public Guid BulkUploadId { get; set; }
        public string Message { get; set; }
        public int TotalRecords { get; set; }
        public string Status { get; set; }
    }

    // DTO for tracking upload status
    public class BulkUploadStatusDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string UploadType { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        public string Status { get; set; }
        public string? ErrorLog { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByName { get; set; }
        public int ProgressPercentage => TotalRecords > 0
            ? (int)((SuccessfulRecords + FailedRecords) * 100.0 / TotalRecords)
            : 0;
    }

    // Internal DTO for parsing Excel rows
    public class CandidateExcelRow
    {
        public int RowNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentLocation { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public int? GraduationYear { get; set; }
        public string Skills { get; set; } // Comma-separated
        public decimal? TotalExperience { get; set; }
        public string CurrentCompany { get; set; }

        // Validation
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsValid => !Errors.Any();
    }

    // DTO for template generation info
    public class ExcelTemplateInfo
    {
        public string[] Headers { get; set; }
        public string[][] SampleData { get; set; }
        public Dictionary<string, string> ColumnDescriptions { get; set; }
    }
}