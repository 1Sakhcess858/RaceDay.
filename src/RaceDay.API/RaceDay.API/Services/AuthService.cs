using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using BCrypt.Net;

namespace RaceDay.API.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
                return null;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (registerDto.Role == "Organiser")
            {
                var organiser = new Organiser
                {
                    OrganiserId = user.UserId,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    OrganiserName = registerDto.OrganiserName ?? $"{registerDto.FirstName}'s Events"
                };
                _context.Organisers.Add(organiser);
            }
            else if (registerDto.Role == "Participant")
            {
                var participant = new Participant
                {
                    ParticipantId = user.UserId,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    DateOfBirth = registerDto.DateOfBirth
                };
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return await GetUserResponseAsync(user.UserId);
        }

        public async Task<UserResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
                return null;

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isValidPassword)
                return null;

            return await GetUserResponseAsync(user.UserId);
        }

        private async Task<UserResponseDto?> GetUserResponseAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return null;

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email
            };

            if (user.Organiser != null)
            {
                response.Role = "Organiser";
                response.FirstName = user.Organiser.FirstName;
                response.LastName = user.Organiser.LastName;
                response.OrganiserName = user.Organiser.OrganiserName;
            }
            else if (user.Participant != null)
            {
                response.Role = "Participant";
                response.FirstName = user.Participant.FirstName;
                response.LastName = user.Participant.LastName;
                response.DateOfBirth = user.Participant.DateOfBirth;
                response.ProfilePictureURL = user.Participant.ProfilePictureURL;
            }

            return response;
        }
    }
}