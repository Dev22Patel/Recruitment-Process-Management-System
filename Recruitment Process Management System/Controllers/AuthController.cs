using Microsoft.AspNetCore.Mvc;
using Recruitment_Process_Management_System.Models.DTOs;
using Recruitment_Process_Management_System.Services;

namespace Recruitment_Process_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly EmailService _emailService;

        public AuthController(AuthService authService,EmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, userId) = await _authService.RegisterAsync(request);

            if (!success)
            {
                return BadRequest(new { Message = message });
            }


            return Ok(new
            {
                Message = message,
                UserId = userId
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, response) = await _authService.LoginAsync(request);

            if (!success)
            {
                return Unauthorized(new { Message = message });
            }

            return Ok(response);
        }
    }
}