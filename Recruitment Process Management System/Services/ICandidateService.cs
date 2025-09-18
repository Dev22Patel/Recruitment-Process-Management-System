using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Services
{
    public interface ICandidateService
    {
        bool IsCandidateProfileComplete(Candidate candidate);
    }
}
