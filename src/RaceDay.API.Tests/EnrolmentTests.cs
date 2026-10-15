using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Tests
{
    [TestClass]
    public class EnrolmentTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [TestMethod]
        public async Task Enrol_ParticipantInEvent_SavesToDatabase()
        {
            // Arrange
            var context = GetDbContext();

            var participantUser = new User { Email = "p@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(participantUser);
            await context.SaveChangesAsync();

            var participant = new Participant { ParticipantId = participantUser.UserId, FirstName = "Alice", LastName = "W" };
            context.Participants.Add(participant);

            var organiserUser = new User { Email = "o@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(organiserUser);
            await context.SaveChangesAsync();

            var organiser = new Organiser { OrganiserId = organiserUser.UserId, FirstName = "John", LastName = "D", OrganiserName = "Club" };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            var eventItem = new Event { OrganiserId = organiser.OrganiserId, Name = "Marathon", EventDate = DateTime.Now.AddDays(30), Location = "Cape Town", Distance = 42, EventType = "Run", CreatedAt = DateTime.Now };
            context.Events.Add(eventItem);
            await context.SaveChangesAsync();

            var category = new Category { EventId = eventItem.EventId, Name = "Open", Description = "Open category" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment = new Enrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = eventItem.EventId,
                CategoryId = category.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Pending"
            };

            // Act
            context.Enrolments.Add(enrolment);
            await context.SaveChangesAsync();

            // Assert
            var savedEnrolment = await context.Enrolments.FirstOrDefaultAsync(e => e.ParticipantId == participant.ParticipantId);
            Assert.IsNotNull(savedEnrolment);
            Assert.AreEqual("Pending", savedEnrolment.Status);
        }

        [TestMethod]
        public async Task Enrol_DuplicateEnrolment_CheckManually()
        {
            // Arrange
            var context = GetDbContext();

            var p = new User { Email = "p@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(p);
            await context.SaveChangesAsync();
            context.Participants.Add(new Participant { ParticipantId = p.UserId, FirstName = "A", LastName = "B" });

            var o = new User { Email = "o@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(o);
            await context.SaveChangesAsync();
            context.Organisers.Add(new Organiser { OrganiserId = o.UserId, FirstName = "J", LastName = "D", OrganiserName = "C" });
            await context.SaveChangesAsync();

            var ev = new Event { OrganiserId = o.UserId, Name = "E", EventDate = DateTime.Now, Location = "L", Distance = 10, EventType = "Run", CreatedAt = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            context.Categories.Add(new Category { EventId = ev.EventId, Name = "C1" });
            await context.SaveChangesAsync();

            var e1 = new Enrolment { ParticipantId = p.UserId, EventId = ev.EventId, CategoryId = 1, Status = "Pending" };
            context.Enrolments.Add(e1);
            await context.SaveChangesAsync();

            // Act - Check if duplicate exists
            var existingEnrolment = await context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == p.UserId && e.EventId == ev.EventId);

            // Assert - A duplicate would be detected
            Assert.IsNotNull(existingEnrolment);
            Assert.AreEqual(1, await context.Enrolments.CountAsync(e => e.ParticipantId == p.UserId && e.EventId == ev.EventId));
        }

        [TestMethod]
        public async Task GetMyEnrolments_ReturnsOnlyMyEnrolments()
        {
            // Arrange
            var context = GetDbContext();

            var p1 = new User { Email = "p1@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            var p2 = new User { Email = "p2@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.AddRange(p1, p2);
            await context.SaveChangesAsync();

            context.Participants.AddRange(
                new Participant { ParticipantId = p1.UserId, FirstName = "A", LastName = "B" },
                new Participant { ParticipantId = p2.UserId, FirstName = "C", LastName = "D" }
            );

            var o = new User { Email = "o@test.com", PasswordHash = "hash", CreatedAt = DateTime.Now };
            context.Users.Add(o);
            await context.SaveChangesAsync();
            context.Organisers.Add(new Organiser { OrganiserId = o.UserId, FirstName = "J", LastName = "D", OrganiserName = "C" });
            await context.SaveChangesAsync();

            var ev = new Event { OrganiserId = o.UserId, Name = "E", EventDate = DateTime.Now, Location = "L", Distance = 10, EventType = "Run", CreatedAt = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            context.Categories.Add(new Category { EventId = ev.EventId, Name = "C1" });
            await context.SaveChangesAsync();

            context.Enrolments.AddRange(
                new Enrolment { ParticipantId = p1.UserId, EventId = ev.EventId, CategoryId = 1, Status = "Pending" },
                new Enrolment { ParticipantId = p2.UserId, EventId = ev.EventId, CategoryId = 1, Status = "Pending" }
            );
            await context.SaveChangesAsync();

            // Act
            var myEnrolments = await context.Enrolments
                .Where(e => e.ParticipantId == p1.UserId)
                .ToListAsync();

            // Assert
            Assert.AreEqual(1, myEnrolments.Count);
            Assert.AreEqual(p1.UserId, myEnrolments[0].ParticipantId);
        }
    }
}