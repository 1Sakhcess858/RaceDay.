using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResults()
        {
            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                .ToListAsync();
            
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null) return NotFound($"Result with ID {id} not found.");
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Participant") return Forbid("Only Participants can view their results.");

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category)
                .Where(r => r.Enrolment.ParticipantId == int.Parse(userId))
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost("enrolment/{enrolmentId}")]
        public async Task<IActionResult> CreateResult(int enrolmentId, [FromBody] Result newResult)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Headers["X-UserId"].FirstOrDefault();
                role = Request.Headers["X-UserRole"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Organiser") return Forbid("Only Organisers can record results.");

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
            
            if (enrolment == null) return NotFound($"Enrolment with ID {enrolmentId} not found.");
            if (enrolment.Event.OrganiserId != int.Parse(userId)) return Forbid("You can only record results for your own events.");

            var existingResult = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentId == enrolmentId);
            if (existingResult != null) return Conflict("Result already recorded for this enrolment.");

            // 👇 Force the ID and clear the navigation property 👇
            newResult.EnrolmentId = enrolmentId;
            newResult.Enrolment = null;

            _context.Results.Add(newResult);
            await _context.SaveChangesAsync();

            enrolment.Status = "Completed";
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResult), new { id = newResult.ResultId }, newResult);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] Result updatedResult)
        {
            if (id != updatedResult.ResultId) return BadRequest("ID mismatch.");
            
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Organiser") return Forbid("Only Organisers can update results.");

            var existingResult = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (existingResult == null) return NotFound($"Result with ID {id} not found.");
            if (existingResult.Enrolment.Event.OrganiserId != int.Parse(userId)) return Forbid("You can only update results for your own events.");

            existingResult.FinishTime = updatedResult.FinishTime;
            existingResult.FinishingPosition = updatedResult.FinishingPosition;

            await _context.SaveChangesAsync();
            return Ok(existingResult);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("You must be logged in.");
            if (role != "Organiser") return Forbid("Only Organisers can delete results.");

            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null) return NotFound($"Result with ID {id} not found.");
            if (result.Enrolment.Event.OrganiserId != int.Parse(userId)) return Forbid("You can only delete results for your own events.");

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}