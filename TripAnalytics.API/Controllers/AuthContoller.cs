using Microsoft.AspNetCore.Mvc;
using TripAnalytics.API.Services;
using TripAnalytics.API.Services.Interfaces;
using TripAnalytics.API.Models;

namespace TripAnalytics.API.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthContoller : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthContoller(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null) return Unauthorized(new { message = "Invalid username or password" });
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            if (response == null) return BadRequest();

            return Ok(response);
        }
    }
}
