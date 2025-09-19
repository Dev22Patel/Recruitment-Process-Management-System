namespace Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment
{
    public class CandidateProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? CurrentLocation { get; set; }
        public decimal? TotalExperience { get; set; }
        public string? CurrentCompany { get; set; }
        public decimal? CurrentSalary { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public int? NoticePeriod { get; set; }
        public string? Source { get; set; }
        public string? CollegeName { get; set; }
        public int? GraduationYear { get; set; }
        public string? Degree { get; set; }
        public string? ResumeFilePath { get; set; }
        public bool IsProfileCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // User information
        public string? UserName { get; set; }
        public string? Email { get; set; }

        // Skills information
        public List<CandidateSkillResponse>? Skills { get; set; }
    }
}
