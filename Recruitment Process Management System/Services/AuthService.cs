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
        private readonly EmailService _emailService;

        public AuthService(
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            IUserRoleRepository userRoleRepository,
            IConfiguration configuration,
            IRoleRepository roleRepository,
            EmailService emailService
            ) 
        {
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _userRoleRepository = userRoleRepository;
            _configuration = configuration;
            _roleRepository = roleRepository;
            _emailService = emailService; 
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
                
                // Get candidate ROLE
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

                // In RegisterAsync, set the required 'User' property when creating the Candidate object
                var candidate = new Candidate
                {
                    Id = Guid.NewGuid(),
                    UserId = createdUser.Id,
                    User = createdUser, // Set the required User property
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

                await _emailService.QueueEmailAsync(createdUser.Email, "Welcome To Recruitment System",
                                    $"Thank you for registering, {createdUser.FirstName}. Your account is now active.");

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

                await _emailService.QueueEmailAsync(user.Email, "Login Notification",
                                    $"Hello {user.FirstName}, you have successfully logged in on {DateTime.UtcNow} UTC.");

                return (true, "Login successful", response);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null);
            }
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT Secret is not configured");

            var key = Encoding.ASCII.GetBytes(jwtSecret);

            // Build claims — use ClaimTypes.Role (standard "role" mapping)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}" ?? string.Empty)
            };

            // Add roles: if employee, add multiple, else add single role
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
                // Candidate or Admin user type
                claims.Add(new Claim(ClaimTypes.Role, user.UserType));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],      
                Audience = _configuration["Jwt:Audience"]  
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}

