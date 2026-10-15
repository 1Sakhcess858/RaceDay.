using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Tests
{
    [TestClass]
    public class EventTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [TestMethod]
        public async Task CreateEvent_ValidData_SavesToDatabase()
        {
            // Arrange
            var context = GetDbContext();

            var user = new User
            {
                Email = "organiser@test.com",
                PasswordHash = "hashed",
                CreatedAt = DateTime.Now
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var organiser = new Organiser
            {
                OrganiserId = user.UserId,
                FirstName = "John",
                LastName = "Doe",
                OrganiserName = "Test Club"
            };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            var newEvent = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = "Test Marathon",
                Description = "A test event",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                Distance = 42.20m,
                EventType = "Run",
                CreatedAt = DateTime.Now
            };

            // Act
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();

            // Assert
            var savedEvent = await context.Events.FirstOrDefaultAsync(e => e.Name == "Test Marathon");
            Assert.IsNotNull(savedEvent);
            Assert.AreEqual("Test Marathon", savedEvent.Name);
            Assert.AreEqual(42.20m, savedEvent.Distance);
            Assert.AreEqual("Run", savedEvent.EventType);
        }

        [TestMethod]
        public async Task GetEvents_ReturnsAllEvents()
        {
            // Arrange
            var context = GetDbContext();

            var user = new User { Email = "org@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var organiser = new Organiser { OrganiserId = user.UserId, FirstName = "J", LastName = "D", OrganiserName = "Club" };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            context.Events.Add(new Event { OrganiserId = organiser.OrganiserId, Name = "Event 1", EventDate = DateTime.Now, Location = "A", Distance = 10, EventType = "Run", CreatedAt = DateTime.Now });
            context.Events.Add(new Event { OrganiserId = organiser.OrganiserId, Name = "Event 2", EventDate = DateTime.Now, Location = "B", Distance = 21, EventType = "Walk", CreatedAt = DateTime.Now });
            await context.SaveChangesAsync();

            // Act
            var events = await context.Events.ToListAsync();

            // Assert
            Assert.AreEqual(2, events.Count);
        }

        [TestMethod]
        public async Task Event_HasOrganiserRelationship()
        {
            // Arrange
            var context = GetDbContext();
            var user = new User { Email = "org@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var organiser = new Organiser { OrganiserId = user.UserId, FirstName = "John", LastName = "Doe", OrganiserName = "Club" };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            var newEvent = new Event { OrganiserId = organiser.OrganiserId, Name = "Test", EventDate = DateTime.Now, Location = "X", Distance = 5, EventType = "Run", CreatedAt = DateTime.Now };
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();

            // Act
            var savedEvent = await context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.Name == "Test");

            // Assert
            Assert.IsNotNull(savedEvent);
            Assert.IsNotNull(savedEvent.Organiser);
            Assert.AreEqual("John", savedEvent.Organiser.FirstName);
        }
    }
}