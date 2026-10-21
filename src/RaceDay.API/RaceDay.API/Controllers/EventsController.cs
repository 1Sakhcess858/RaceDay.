using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Participant)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Category)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Result)
                .OrderBy(e => e.EventDate)
                .Select(e => new 
                {
                    e.EventId,
                    e.OrganiserId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.BannerImageURL,
                    e.CreatedAt,
                    OrganiserName = e.Organiser != null ? e.Organiser.OrganiserName : "Unknown",
                    Categories = e.Categories,
                    Enrolments = e.Enrolments.Select(en => new 
                    {
                        en.EnrolmentId,
                        en.ParticipantId,
                        ParticipantName = en.Participant != null ? (en.Participant.FirstName + " " + en.Participant.LastName) : "Unknown",
                        en.CategoryId,
                        CategoryName = en.Category != null ? en.Category.Name : "Unknown",
                        en.Status,
                        en.EnrolmentDate,
                        Result = en.Result != null ? new 
                        {
                            en.Result.ResultId,
                            en.Result.FinishTime,
                            en.Result.FinishingPosition
                        } : null
                    })
                })
                .ToListAsync();
            
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Participant)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Category)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Result)
                .Where(e => e.EventId == id)
                .Select(e => new 
                {
                    e.EventId,
                    e.OrganiserId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.BannerImageURL,
                    e.CreatedAt,
                    OrganiserName = e.Organiser != null ? e.Organiser.OrganiserName : "Unknown",
                    Categories = e.Categories,
                    Enrolments = e.Enrolments.Select(en => new 
                    {
                        en.EnrolmentId,
                        en.ParticipantId,
                        ParticipantName = en.Participant != null ? (en.Participant.FirstName + " " + en.Participant.LastName) : "Unknown",
                        en.CategoryId,
                        CategoryName = en.Category != null ? en.Category.Name : "Unknown",
                        en.Status,
                        en.EnrolmentDate,
                        Result = en.Result != null ? new 
                        {
                            en.Result.ResultId,
                            en.Result.FinishTime,
                            en.Result.FinishingPosition
                        } : null
                    })
                })
                .FirstOrDefaultAsync();

            if (eventItem == null)
                return NotFound($"Event with ID {id} not found.");

            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-UserId"].FirstOrDefault();
                role = Request.Headers["X-UserRole"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Organiser") return Forbid("Only Organisers can create events.");

            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.OrganiserId == int.Parse(userId));
            if (organiser == null) return BadRequest("Organiser profile not found.");

            newEvent.OrganiserId = organiser.OrganiserId;
            newEvent.CreatedAt = DateTime.Now;

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            if (id != updatedEvent.EventId) return BadRequest("ID mismatch.");
            
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Organiser") return Forbid("Only Organisers can update events.");

            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (existingEvent == null) return NotFound($"Event with ID {id} not found.");
            if (existingEvent.OrganiserId != int.Parse(userId)) return Forbid("You can only update your own events.");

            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.Distance = updatedEvent.Distance;
            existingEvent.EventType = updatedEvent.EventType;

            await _context.SaveChangesAsync();
            return Ok(existingEvent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Organiser") return Forbid("Only Organisers can delete events.");

            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (eventItem == null) return NotFound($"Event with ID {id} not found.");
            if (eventItem.OrganiserId != int.Parse(userId)) return Forbid("You can only delete your own events.");

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}