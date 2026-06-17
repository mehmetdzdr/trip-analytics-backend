using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using TripAnalytics.API.Domain.Entities;
using TripAnalytics.API.Models;
using TripAnalytics.API.Repositories.Interfaces;
using TripAnalytics.API.Services;
using Xunit;

namespace TripAnalytics.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Jwt:Secret"]).Returns("test-secret-key-minimum-32-characters-long");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _authService = new AuthService(_userRepositoryMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsNull()
        {
            var request = new LoginRequest { username = "mocktestuser", password = "random123" };

            //user yok - repodan null gelmeli
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(request.username))
                .ReturnsAsync((User?)null);

            var result = await _authService.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            var existingUser = new User
            {
                Id = 1,
                username = "mocktestuser",
                email = "test@test.com",
                passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            var request = new LoginRequest { username = "mocktestuser", password = "wrongpassword" };
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(request.username))
                .ReturnsAsync(existingUser);

            var result = await _authService.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseWithToken()
        {
            var existingUser = new User
            {
                Id = 1,
                username = "mocktestuser",
                email = "test@test.com",
                passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            var request = new LoginRequest { username = "mocktestuser", password = "correctpassword" };
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(request.username))
                .ReturnsAsync(existingUser);

            var result = await _authService.LoginAsync(request);

            Assert.NotNull(result);
            Assert.Equal("mocktestuser", result!.username);
            Assert.False(string.IsNullOrEmpty(result.token));
        }

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsNull()
        {
            var existingUser = new User
            {
                Id = 1,
                username = "mocktestuser",
                email = "mehmet@test.com",
                passwordHash = "somehash"
            };

            var request = new RegisterRequest { username = "mocktestuser", email = "new@test.com", password = "newpassword" };
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(request.username))
                .ReturnsAsync(existingUser);

            var result = await _authService.RegisterAsync(request);

            Assert.Null(result);
            _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_NewUsername_CreatesUserAndReturnsAuthResponse()
        {
            var request = new RegisterRequest { username = "newuser", email = "newuser@test.com", password = "password123" };

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(request.username))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var result = await _authService.RegisterAsync(request);

            Assert.NotNull(result);
            Assert.Equal("newuser", result!.username);
            Assert.False(string.IsNullOrEmpty(result.token));
            _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
                u.username == "newuser" && u.email == "newuser@test.com")), Times.Once); //check whether is imnserted to db or not
        }
    }
}
