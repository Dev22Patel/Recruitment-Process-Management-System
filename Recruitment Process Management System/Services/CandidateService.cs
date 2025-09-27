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
            // Define minimum required fields for a complete profile
            var requiredFieldsComplete = !string.IsNullOrWhiteSpace(candidate.CurrentLocation) &&
                                        candidate.TotalExperience.HasValue &&
                                        !string.IsNullOrWhiteSpace(candidate.CurrentCompany) &&
                                        candidate.CurrentSalary.HasValue &&
                                        candidate.ExpectedSalary.HasValue &&
                                        candidate.NoticePeriod.HasValue &&
                                        !string.IsNullOrWhiteSpace(candidate.CollegeName) &&
                                        candidate.GraduationYear.HasValue &&
                                        !string.IsNullOrWhiteSpace(candidate.Degree);

            // Optionally, you can also check if at least one skill is added
            var hasSkills = candidate.CandidateSkills?.Any() == true;

            return requiredFieldsComplete; // You can add && hasSkills if skills are mandatory
        }
    }
}
