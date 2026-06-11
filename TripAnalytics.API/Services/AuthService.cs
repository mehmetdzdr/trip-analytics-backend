using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TripAnalytics.API.Domain.Entities;
using TripAnalytics.API.Models;
using TripAnalytics.API.Repositories.Interfaces;
using TripAnalytics.API.Services.Interfaces;

namespace TripAnalytics.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.username);
            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(request.password, user.passwordHash))
                return null;

            return new AuthResponse
            {
                token = GenerateToken(user),
                username = request.username

            };
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepository.GetByUsernameAsync(request.username);
            if (existing != null) return null;

            var user = new User
            {
                username = request.username,
                email = request.email,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password)
            };
            await _userRepository.CreateAsync(user);
            return new AuthResponse
            {
                token = GenerateToken(user),
                username = user.username
            };
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
