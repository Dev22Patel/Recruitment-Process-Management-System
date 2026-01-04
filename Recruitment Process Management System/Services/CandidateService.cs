using Recruitment_Process_Management_System.Models.DTOs.Candidate_Managment;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;

        public CandidateService(ICandidateRepository candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        public bool IsCandidateProfileComplete(Candidate candidate)
        {
            return _candidateRepository.IsProfileComplete(candidate);
        }

        public async Task<Candidate?> UpdateCandidateProfileAsync(Guid userId, UpdateCandidate updateDto)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null) return null;

            // Update candidate properties
            candidate.CurrentLocation = updateDto.CurrentLocation;
            candidate.TotalExperience = updateDto.TotalExperience;
            candidate.CurrentCompany = updateDto.CurrentCompany;
            candidate.CurrentSalary = updateDto.CurrentSalary;
            candidate.ExpectedSalary = updateDto.ExpectedSalary;
            candidate.NoticePeriod = updateDto.NoticePeriod;
            candidate.CollegeName = updateDto.CollegeName;
            candidate.GraduationYear = updateDto.GraduationYear;
            candidate.Degree = updateDto.Degree;
            candidate.ResumeFilePath = updateDto.ResumeFilePath;

            // Update candidate basic info
            var updatedCandidate = await _candidateRepository.UpdateAsync(candidate);

            // Update skills if provided
            if (updateDto.Skills != null && updateDto.Skills.Any())
            {
                var candidateSkills = updateDto.Skills.Select(s => new Models.Entities.CandidateSkill
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidate.Id,
                    SkillId = s.SkillId,
                    YearsOfExperience = s.YearsOfExperience,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _candidateRepository.UpdateCandidateSkillsAsync(candidate.Id, candidateSkills);
            }

            // Check and update profile completion status
            var isProfileComplete = ValidateProfileCompletion(updatedCandidate);
            await _candidateRepository.UpdateProfileCompletionStatusAsync(candidate.Id, isProfileComplete);

            // Return updated candidate with includes
            return await _candidateRepository.GetByUserIdAsync(userId);
        }

        public CandidateProfile MapToProfileDto(Candidate candidate)
        {
            return new CandidateProfile
            {
                Id = candidate.Id,
                UserId = candidate.UserId,
                CurrentLocation = candidate.CurrentLocation,
                TotalExperience = candidate.TotalExperience,
                CurrentCompany = candidate.CurrentCompany,
                CurrentSalary = candidate.CurrentSalary,
                ExpectedSalary = candidate.ExpectedSalary,
                NoticePeriod = candidate.NoticePeriod,
                CollegeName = candidate.CollegeName,
                GraduationYear = candidate.GraduationYear,
                Degree = candidate.Degree,
                ResumeFilePath = candidate.ResumeFilePath,
                IsProfileCompleted = candidate.IsProfileCompleted,
                CreatedAt = candidate.CreatedAt,
                UserName = candidate.User?.FirstName,
                Email = candidate.User?.Email,
                Skills = candidate.CandidateSkills?.Select(cs => new CandidateSkillResponse
                {
                    SkillId = cs.SkillId,
                    SkillName = cs.Skill?.SkillName ?? "",
                    Category = cs.Skill?.Category ?? "",
                    YearsOfExperience = cs.YearsOfExperience
                }).ToList()
            };
        }

        public bool ValidateProfileCompletion(Candidate candidate)
        {
            // Checking  if candidate is a fresher (experience che ke nai or 0 experience che)  
            bool isFresher = !candidate.TotalExperience.HasValue || candidate.TotalExperience == 0;

            // Required fields for ALL candidates (both fresher and experienced)
            bool basicFieldsComplete =
                !string.IsNullOrWhiteSpace(candidate.CurrentLocation) &&
                !string.IsNullOrWhiteSpace(candidate.CollegeName) &&
                candidate.GraduationYear.HasValue &&
                !string.IsNullOrWhiteSpace(candidate.Degree) &&
                candidate.ExpectedSalary.HasValue;

            // For EXPERIENCED candidates, these additional fields are required
            bool experienceFieldsComplete = true;
            if (!isFresher)
            {
                experienceFieldsComplete =
                    !string.IsNullOrWhiteSpace(candidate.CurrentCompany) &&
                    candidate.CurrentSalary.HasValue &&
                    candidate.NoticePeriod.HasValue;
            }

            return basicFieldsComplete && experienceFieldsComplete;
        }
    }
}
