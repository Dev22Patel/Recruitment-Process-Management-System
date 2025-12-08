using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Repositories.Implementations;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using Recruitment_Process_Management_System.Services;
using Recruitment_Process_Management_System.Services.CloudinaryService;
using Recruitment_Process_Management_System.Services.RabbitMq;
using System.Text;

namespace Recruitment_Process_Management_System.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .EnableSensitiveDataLogging(true)); // For debugging

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IJobPositionRepository, JobPositionRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IInterviewFeedbackRepository,InterviewFeedbackRepository>();
            services.AddScoped<IInterviewParticipantRepository, InterviewParticipantRepository>();
            services.AddScoped<IInterviewRoundRepository, InterviewRoundRepository>();

            // Register services with their interfaces
            services.AddScoped<SkillService>();
            services.AddScoped<AuthService>();
            services.AddScoped<EmailService>();
            services.AddScoped<JobPositionService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IRabbitMqService, RabbitMqService>();
            services.AddScoped<RoleService>();
            services.AddScoped<AdminService>();
            services.AddScoped<ApplicationService>();
            services.AddScoped<JobService>();
            services.AddScoped<InterviewService>();

            return services;
        }
    }
}