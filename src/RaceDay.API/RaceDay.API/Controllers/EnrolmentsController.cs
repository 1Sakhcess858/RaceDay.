using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/enrolments
        [HttpGet]
        public async Task<IActionResult> GetAllEnrolments()
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .ToListAsync();
            
            return Ok(enrolments);
        }

        // GET: api/enrolments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
                return NotFound($"Enrolment with ID {id} not found.");

            return Ok(enrolment);
        }

        // GET: api/enrolments/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");
            
            if (role != "Participant")
                return Forbid("Only Participants can view their enrolments.");

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == int.Parse(userId))
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.EventId,
                    EventName = e.Event != null ? e.Event.Name : "Unknown",
                    e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.Name : "Unknown",
                    e.EnrolmentDate,
                    e.Status,
                    Result = e.Result != null ? new
                    {
                        e.Result.ResultId,
                        e.Result.FinishTime,
                        e.Result.FinishingPosition
                    } : null
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEnrolmentsByEvent(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");
            
            if (role != "Organiser")
                return Forbid("Only Organisers can view event enrolments.");

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            
            if (eventItem == null)
                return NotFound($"Event with ID {eventId} not found.");

            if (eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only view enrolments for your own events.");

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.EventId == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/enrolments/event/{eventId}/enrol
        [HttpPost("event/{eventId}/enrol")]
        public async Task<IActionResult> EnrolInEvent(int eventId, [FromBody] int categoryId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");
            
            if (role != "Participant")
                return Forbid("Only Participants can enrol in events.");

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            
            if (eventItem == null)
                return NotFound($"Event with ID {eventId} not found.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.EventId == eventId);
            
            if (category == null)
                return BadRequest("Category not found for this event.");

            var existingEnrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == int.Parse(userId) && e.EventId == eventId);
            
            if (existingEnrolment != null)
                return Conflict("You are already enrolled in this event.");

            var enrolment = new Enrolment
            {
                ParticipantId = int.Parse(userId),
                EventId = eventId,
                CategoryId = categoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrolmentId }, enrolment);
        }

        // PUT: api/enrolments/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateEnrolmentStatus(int id, [FromBody] string status)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");
            
            if (role != "Organiser")
                return Forbid("Only Organisers can update enrolment status.");

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
                return NotFound($"Enrolment with ID {id} not found.");

            if (enrolment.Event.OrganiserId != int.Parse(userId))
                return Forbid("You can only update enrolments for your own events.");

            if (status != "Pending" && status != "Confirmed" && status != "Completed")
                return BadRequest("Invalid status. Allowed values: Pending, Confirmed, Completed.");

            enrolment.Status = status;
            await _context.SaveChangesAsync();

            return Ok(enrolment);
        }
    }
}