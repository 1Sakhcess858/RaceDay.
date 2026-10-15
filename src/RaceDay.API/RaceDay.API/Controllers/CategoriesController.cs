using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Event)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound($"Category with ID {id} not found.");

            return Ok(category);
        }

        // GET: api/categories/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetCategoriesByEvent(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            if (!categories.Any())
                return NotFound($"No categories found for event ID {eventId}.");

            return Ok(categories);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category newCategory)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return Forbid("Only Organisers can create categories.");

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == newCategory.EventId);

            if (eventItem == null)
                return BadRequest("Event not found.");

            if (eventItem.OrganiserId != int.Parse(userId))
                return Forbid("You can only add categories to your own events.");

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.CategoryId }, newCategory);
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
        {
            if (id != updatedCategory.CategoryId)
                return BadRequest("ID mismatch.");

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return Forbid("Only Organisers can update categories.");

            var existingCategory = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (existingCategory == null)
                return NotFound($"Category with ID {id} not found.");

            if (existingCategory.Event.OrganiserId != int.Parse(userId))
                return Forbid("You can only update categories for your own events.");

            existingCategory.Name = updatedCategory.Name;
            existingCategory.Description = updatedCategory.Description;

            await _context.SaveChangesAsync();
            return Ok(existingCategory);
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return Forbid("Only Organisers can delete categories.");

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound($"Category with ID {id} not found.");

            if (category.Event.OrganiserId != int.Parse(userId))
                return Forbid("You can only delete categories for your own events.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}