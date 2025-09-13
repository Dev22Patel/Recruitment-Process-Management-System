using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Repositories.Implementations;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using Recruitment_Process_Management_System.Services;
using System.Text;

namespace Recruitment_Process_Management_System.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            // Register services
            services.AddScoped<AuthService>();

            return services;
        }
    }
}
