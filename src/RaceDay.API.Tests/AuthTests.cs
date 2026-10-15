using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;
using RaceDay.API.DTOs;

namespace RaceDay.API.Tests
{
    [TestClass]
    public class AuthTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [TestMethod]
        public async Task Register_ValidParticipant_ReturnsUserResponse()
        {
            // Arrange
            var context = GetDbContext();
            var authService = new AuthService(context);
            var registerDto = new RegisterDto
            {
                Email = "participant@test.com",
                Password = "Password123!",
                Role = "Participant",
                FirstName = "Alice",
                LastName = "Wonder"
            };

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("participant@test.com", result.Email);
            Assert.AreEqual("Participant", result.Role);
            Assert.AreEqual("Alice", result.FirstName);
            Assert.AreEqual("Wonder", result.LastName);
        }

        [TestMethod]
        public async Task Register_ValidOrganiser_ReturnsUserResponse()
        {
            // Arrange
            var context = GetDbContext();
            var authService = new AuthService(context);
            var registerDto = new RegisterDto
            {
                Email = "organiser@test.com",
                Password = "Password123!",
                Role = "Organiser",
                FirstName = "John",
                LastName = "Doe",
                OrganiserName = "Test Events Club"
            };

            // Act
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("organiser@test.com", result.Email);
            Assert.AreEqual("Organiser", result.Role);
            Assert.AreEqual("Test Events Club", result.OrganiserName);
        }

        [TestMethod]
        public async Task Register_DuplicateEmail_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var authService = new AuthService(context);
            var registerDto = new RegisterDto
            {
                Email = "duplicate@test.com",
                Password = "Password123!",
                Role = "Participant",
                FirstName = "Test",
                LastName = "User"
            };
            await authService.RegisterAsync(registerDto);

            // Act - Try to register same email again
            var result = await authService.RegisterAsync(registerDto);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsUserResponse()
        {
            // Arrange
            var context = GetDbContext();
            var authService = new AuthService(context);
            await authService.RegisterAsync(new RegisterDto
            {
                Email = "login@test.com",
                Password = "Password123!",
                Role = "Participant",
                FirstName = "Test",
                LastName = "User"
            });

            var loginDto = new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("login@test.com", result.Email);
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var authService = new AuthService(context);
            await authService.RegisterAsync(new RegisterDto
            {
                Email = "login@test.com",
                Password = "Password123!",
                Role = "Participant",
                FirstName = "Test",
                LastName = "User"
            });

            var loginDto = new LoginDto
            {
                Email = "login@test.com",
                Password = "WrongPassword!"
            };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Login_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var authService = new AuthService(context);
            var loginDto = new LoginDto
            {
                Email = "nonexistent@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            Assert.IsNull(result);
        }
    }
}