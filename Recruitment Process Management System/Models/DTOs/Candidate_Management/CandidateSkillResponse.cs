namespace Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment
{
    public class CandidateSkillResponse
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? YearsOfExperience { get; set; }
    }
}
