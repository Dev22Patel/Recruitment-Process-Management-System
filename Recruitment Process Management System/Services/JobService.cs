using Recruitment_Process_Management_System.Models.DTOs.Application_Management;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class JobService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IApplicationRepository _applicationRepository;

        public JobService(
            ICandidateRepository candidateRepository,
            IJobPositionRepository jobPositionRepository,
            IApplicationRepository applicationRepository)
        {
            _candidateRepository = candidateRepository;
            _jobPositionRepository = jobPositionRepository;
            _applicationRepository = applicationRepository;
        }

        public async Task<EligibilityCheckResult> CheckEligibilityAsync(Guid userId, Guid jobPositionId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null)
                return new EligibilityCheckResult { IsEligible = false, Message = "Candidate profile not found" };

            if (!candidate.IsProfileCompleted)
                return new EligibilityCheckResult { IsEligible = false, Message = "Please complete your profile before checking eligibility" };

            var jobPosition = await _jobPositionRepository.GetByIdAsync(jobPositionId);
            if (jobPosition == null)
                return new EligibilityCheckResult { IsEligible = false, Message = "Job position not found" };

            if (jobPosition.StatusId != 1)
                return new EligibilityCheckResult { IsEligible = false, Message = "This job is no longer open" };

            var existingApp = await _applicationRepository.GetByJobAndCandidateAsync(jobPositionId, candidate.Id);
            if (existingApp != null)
                return new EligibilityCheckResult { IsEligible = false, Message = "You have already applied for this position" };

            // Experience validation
            var candidateExperience = candidate.TotalExperience ?? 0;
            var requiredExperience = jobPosition.MinExperience ?? 0;

            if (candidateExperience < requiredExperience)
            {
                return new EligibilityCheckResult
                {
                    IsEligible = false,
                    Message = $"Insufficient experience. Required: {requiredExperience}+ years, You have: {candidateExperience} years",
                    SkillMatchPercentage = 0,
                    MatchedRequiredSkills = 0,
                    TotalRequiredSkills = 0,
                    MatchedPreferredSkills = 0,
                    TotalPreferredSkills = 0
                };
            }

            // Skill matching logic
            var candidateSkills = candidate.CandidateSkills?
                .Select(cs => cs.Skill?.SkillName?.ToLower().Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToHashSet() ?? new HashSet<string>();

            var requiredSkills = jobPosition.JobSkillRequirements?
                .Where(jsr => jsr.IsRequired && jsr.Skill?.SkillName != null)
                .Select(jsr => jsr.Skill.SkillName.ToLower().Trim())
                .ToList() ?? new List<string>();

            var preferredSkills = jobPosition.JobSkillRequirements?
                .Where(jsr => !jsr.IsRequired && jsr.Skill?.SkillName != null)
                .Select(jsr => jsr.Skill.SkillName.ToLower().Trim())
                .ToList() ?? new List<string>();

            var matchedRequired = requiredSkills.Count(s => candidateSkills.Contains(s));
            var matchedPreferred = preferredSkills.Count(s => candidateSkills.Contains(s));

            var skillMatchPercentage = requiredSkills.Count > 0
                ? (int)Math.Round((double)matchedRequired / requiredSkills.Count * 100)
                : 100;

            // Check if eligible based on skill match (70% threshold)
            var isEligible = requiredSkills.Count == 0 || skillMatchPercentage >= 70;

            var message = !isEligible && requiredSkills.Count > 0
                ? $"You need at least 70% match on required skills. Current: {skillMatchPercentage}%"
                : null;

            return new EligibilityCheckResult
            {
                IsEligible = isEligible,
                Message = message,
                SkillMatchPercentage = skillMatchPercentage,
                MatchedRequiredSkills = matchedRequired,
                TotalRequiredSkills = requiredSkills.Count,
                MatchedPreferredSkills = matchedPreferred,
                TotalPreferredSkills = preferredSkills.Count
            };
        }
    }
}