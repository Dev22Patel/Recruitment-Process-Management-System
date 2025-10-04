namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class JobListingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? ExperienceLevel { get; set; }
        public string ExperienceRange { get; set; } // e.g., "2-5 years"
        public decimal? Salary { get; set; }
        public DateTime PostedDate { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> PreferredSkills { get; set; } = new();
    }
}
