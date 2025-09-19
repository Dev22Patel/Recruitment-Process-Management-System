using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment
{
    public class UpdateCandidate
    {
        [MaxLength(100)]
        public string? CurrentLocation { get; set; }

        [Range(0, 50)]
        public decimal? TotalExperience { get; set; }

        [MaxLength(200)]
        public string? CurrentCompany { get; set; }

        [Range(0, 99999999.99)]
        public decimal? CurrentSalary { get; set; }

        [Range(0, 99999999.99)]
        public decimal? ExpectedSalary { get; set; }

        [Range(0, 365)]
        public int? NoticePeriod { get; set; }

        [MaxLength(50)]
        public string? Source { get; set; }

        [MaxLength(200)]
        public string? CollegeName { get; set; }

        [Range(1900, 2030)]
        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? Degree { get; set; }

        [MaxLength(500)]
        public string? ResumeFilePath { get; set; }

        // Skills with experience
        public List<CandidateSkill>? Skills { get; set; }
    }
}
