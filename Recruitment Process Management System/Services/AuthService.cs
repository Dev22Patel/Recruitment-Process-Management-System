using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Models.Entities;
using Recruitment_Process_Management_System.Repositories.Interfaces;
using Recruitment_Process_Management_System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Recruitment_Process_Management_System.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;

        public AuthService(
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            IUserRoleRepository userRoleRepository,
            IConfiguration configuration,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _userRoleRepository = userRoleRepository;
            _configuration = configuration;
            _roleRepository = roleRepository;
        }

        public async Task<(bool Success, string Message, Guid? UserId)> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return (false, "Email already registered", null);
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                
                //get candidate ROLE
                var CandidateRole = await _roleRepository.GetByNameAsync("Candidate");
                if (CandidateRole == null)
                {
                    return (false, "Candidate role not found", null);
                }

                // Create user with GUID
                var user = new User
                {
                    Id = Guid.NewGuid(), // Generate new GUID
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email.ToLower().Trim(),
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = passwordHash,
                    UserType = CandidateRole.RoleName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Create candidate profile with matching GUID
                var candidate = new Candidate
                {
                    Id = Guid.NewGuid(),
                    UserId = createdUser.Id, 
                    Source = "Manual Entry",
                    IsProfileCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _candidateRepository.CreateAsync(candidate);

                var userRole = new UserRole
                {
                    UserId = createdUser.Id,
                    RoleId = CandidateRole.Id, 
                    AssignedAt = DateTime.UtcNow
                };

                await _userRoleRepository.CreateAsync(userRole);

                return (true, "Registration successful", createdUser.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request)
        {
            try
            {
                // Normalize email for consistent lookup
                var email = request.Email.ToLower().Trim();
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    return (false, "Invalid email or password", null);
                }

                // Check if user has a password (registered user)
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return (false, "Account not activated. Please contact administrator.", null);
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return (false, "Invalid email or password", null);
                }

                if (!user.IsActive)
                {
                    return (false, "Account is deactivated", null);
                }

                // Generate JWT token
                var token = await GenerateJwtTokenAsync(user);

                var response = new LoginResponse
                {
                    UserId = user.Id.ToString(), // Convert Guid to string for response
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserType = user.UserType,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                };

                return (true, "Login successful", response);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null);
            }
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecret = _configuration["Jwt:Key"]; 

                if (string.IsNullOrEmpty(jwtSecret))
                {
                    throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
                }

                var key = Encoding.ASCII.GetBytes(jwtSecret);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Convert Guid to string
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim("UserType", user.UserType)
                };

                // Only get roles for Employee users (simplified approach)
                if (user.UserType == "Employee")
                {
                    var roles = await _userRoleRepository.GetRolesByUserIdAsync(user.Id);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
                else
                {
                    // For candidates, use UserType as role
                    claims.Add(new Claim(ClaimTypes.Role, user.UserType));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Token generation failed: {ex.Message}", ex);
            }
        }
    }
}