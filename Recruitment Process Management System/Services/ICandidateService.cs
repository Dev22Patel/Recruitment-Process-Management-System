using Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment;
using Recruitment_Process_Management_System.Models.Entities;

namespace Recruitment_Process_Management_System.Services
{
    public interface ICandidateService
    {
        bool IsCandidateProfileComplete(Candidate candidate);
        Task<Candidate?> UpdateCandidateProfileAsync(Guid userId, UpdateCandidate updateDto);
        CandidateProfile MapToProfileDto(Candidate candidate);
        bool ValidateProfileCompletion(Candidate candidate);
    }
}
