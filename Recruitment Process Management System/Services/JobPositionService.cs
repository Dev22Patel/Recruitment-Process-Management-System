using Recruitment_Process_Management_System.Models.DTOs.Job_Management;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;

namespace Recruitment_Process_Management_System.Services
{
    public class JobPositionService
    {
        private readonly IJobPositionRepository _jobPositionRepository;

        public JobPositionService(IJobPositionRepository jobPositionRepository)
        {
            _jobPositionRepository = jobPositionRepository;
        }

        public async Task<JobPositionResponseDto> CreateJobPositionAsync(CreateJobPositionDto dto, Guid createdBy)
        {
            // Create job position entity
            var jobPosition = new JobPosition
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Department = dto.Department,
                Location = dto.Location,
                EmploymentType = dto.EmploymentType,
                ExperienceLevel = dto.ExperienceLevel,
                MinExperience = dto.MinExperience,
                Salary = dto.Salary,
                StatusId = dto.StatusId,
                StatusReason = dto.StatusReason,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            // Save job position
            var createdJob = await _jobPositionRepository.CreateAsync(jobPosition);

            // Add required skills
            if (dto.RequiredSkills?.Any() == true)
            {
                var requiredSkills = dto.RequiredSkills.Select(s => new JobSkillRequirement
                {
                    Id = Guid.NewGuid(),
                    JobPositionId = createdJob.Id,
                    SkillId = s.SkillId,
                    IsRequired = true,
                    MinYearsExperience = s.MinYearsExperience
                }).ToList();

                await _jobPositionRepository.AddSkillRequirementsAsync(requiredSkills);
            }

            // Add preferred skills
            if (dto.PreferredSkills?.Any() == true)
            {
                var preferredSkills = dto.PreferredSkills.Select(s => new JobSkillRequirement
                {
                    Id = Guid.NewGuid(),
                    JobPositionId = createdJob.Id,
                    SkillId = s.SkillId,
                    IsRequired = false,
                    MinYearsExperience = s.MinYearsExperience
                }).ToList();

                await _jobPositionRepository.AddSkillRequirementsAsync(preferredSkills);
            }

           

            // Fetch and return complete job position
            var result = await _jobPositionRepository.GetByIdAsync(createdJob.Id);
            return MapToResponseDto(result!);
        }

        public async Task<JobPositionResponseDto?> GetJobPositionByIdAsync(Guid id)
        {
            var jobPosition = await _jobPositionRepository.GetByIdAsync(id);
            return jobPosition == null ? null : MapToResponseDto(jobPosition);
        }

        public async Task<List<JobPositionResponseDto>> GetAllJobPositionsAsync()
        {
            var jobPositions = await _jobPositionRepository.GetAllAsync();
            return jobPositions.Select(MapToResponseDto).ToList();
        }

        public async Task<List<JobListingDto>> GetActiveJobListingsAsync()
        {
            var activeJobs = await _jobPositionRepository.GetActiveJobsAsync();
            return activeJobs.Select(MapToListingDto).ToList();
        }

        public async Task<JobPositionResponseDto?> UpdateJobPositionAsync(UpdateJobPositionDto dto, Guid updatedBy)
        {
            var existingJob = await _jobPositionRepository.GetByIdAsync(dto.Id);
            if (existingJob == null) return null;

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(dto.Title))
                existingJob.Title = dto.Title;
            if (dto.Description != null)
                existingJob.Description = dto.Description;
            if (dto.Department != null)
                existingJob.Department = dto.Department;
            if (dto.Location != null)
                existingJob.Location = dto.Location;
            if (dto.EmploymentType != null)
                existingJob.EmploymentType = dto.EmploymentType;
            if (dto.ExperienceLevel != null)
                existingJob.ExperienceLevel = dto.ExperienceLevel;
            if (dto.MinExperience.HasValue)
                existingJob.MinExperience = dto.MinExperience;
            if (dto.Salary.HasValue)
                existingJob.Salary = dto.Salary;
            if (dto.StatusId.HasValue)
                existingJob.StatusId = dto.StatusId.Value;
            if (dto.StatusReason != null)
                existingJob.StatusReason = dto.StatusReason;

            await _jobPositionRepository.UpdateAsync(existingJob);

            // Update skills if provided
            if (dto.RequiredSkills != null || dto.PreferredSkills != null)
            {
                await _jobPositionRepository.RemoveSkillRequirementsAsync(dto.Id);

                var allSkills = new List<JobSkillRequirement>();

                if (dto.RequiredSkills?.Any() == true)
                {
                    allSkills.AddRange(dto.RequiredSkills.Select(s => new JobSkillRequirement
                    {
                        Id = Guid.NewGuid(),
                        JobPositionId = dto.Id,
                        SkillId = s.SkillId,
                        IsRequired = true,
                        MinYearsExperience = s.MinYearsExperience
                    }));
                }

                if (dto.PreferredSkills?.Any() == true)
                {
                    allSkills.AddRange(dto.PreferredSkills.Select(s => new JobSkillRequirement
                    {
                        Id = Guid.NewGuid(),
                        JobPositionId = dto.Id,
                        SkillId = s.SkillId,
                        IsRequired = false,
                        MinYearsExperience = s.MinYearsExperience
                    }));
                }

                if (allSkills.Any())
                {
                    await _jobPositionRepository.AddSkillRequirementsAsync(allSkills);
                }
            }

           

            var result = await _jobPositionRepository.GetByIdAsync(dto.Id);
            return MapToResponseDto(result!);
        }

        public async Task<bool> DeleteJobPositionAsync(Guid id)
        {
            return await _jobPositionRepository.DeleteAsync(id);
        }

        // Helper methods
        private JobPositionResponseDto MapToResponseDto(JobPosition job)
        {
            return new JobPositionResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Department = job.Department,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                ExperienceLevel = job.ExperienceLevel,
                MinExperience = job.MinExperience,
                Salary = job.Salary,
                StatusId = job.StatusId,
                StatusName = job.Status?.StatusName,
                StatusReason = job.StatusReason,
                CreatedBy = job.CreatedBy,
                CreatedByName = job.Creator != null ? $"{job.Creator.FirstName} {job.Creator.LastName}" : null,
                CreatedAt = job.CreatedAt,
                RequiredSkills = job.JobSkillRequirements
                    .Where(jsr => jsr.IsRequired)
                    .Select(jsr => new SkillRequirementResponseDto
                    {
                        SkillId = jsr.SkillId,
                        SkillName = jsr.Skill?.SkillName ?? "",
                        Category = jsr.Skill?.Category,
                        MinYearsExperience = jsr.MinYearsExperience
                    }).ToList(),
                PreferredSkills = job.JobSkillRequirements
                    .Where(jsr => !jsr.IsRequired)
                    .Select(jsr => new SkillRequirementResponseDto
                    {
                        SkillId = jsr.SkillId,
                        SkillName = jsr.Skill?.SkillName ?? "",
                        Category = jsr.Skill?.Category,
                        MinYearsExperience = jsr.MinYearsExperience
                    }).ToList(),
                
            };
        }

        private JobListingDto MapToListingDto(JobPosition job)
        {
            return new JobListingDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Department = job.Department,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                ExperienceLevel = job.ExperienceLevel,
                ExperienceRange = job.MinExperience?.ToString() ?? string.Empty,
                Salary = job.Salary,
                PostedDate = job.CreatedAt,
                RequiredSkills = job.JobSkillRequirements
                    .Where(jsr => jsr.IsRequired)
                    .Select(jsr => jsr.Skill?.SkillName ?? "")
                    .ToList(),
                PreferredSkills = job.JobSkillRequirements
                    .Where(jsr => !jsr.IsRequired)
                    .Select(jsr => jsr.Skill?.SkillName ?? "")
                    .ToList()
            };
        }
    }
}
