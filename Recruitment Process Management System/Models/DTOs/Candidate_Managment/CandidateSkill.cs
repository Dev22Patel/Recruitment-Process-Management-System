using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment
{
    public class CandidateSkill
    {
        [Required]
        public Guid SkillId { get; set; }

        [Range(0, 50)]
        public decimal? YearsOfExperience { get; set; }
    }
}
