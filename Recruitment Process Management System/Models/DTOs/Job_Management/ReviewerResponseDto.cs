namespace Recruitment_Process_Management_System.Models.DTOs.Job_Management
{
    public class ReviewerResponseDto
    {
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerEmail { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
