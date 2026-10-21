using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.DTOs;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var events = await _apiService.GetAsync<List<EventDto>>("api/Events");
            return View(events ?? new List<EventDto>());
        }

        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View(new EventDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventDto newEvent)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var response = await _apiService.PostAsync("api/Events", newEvent);
            if (response.IsSuccessStatusCode) return RedirectToAction("Dashboard");

            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error ({response.StatusCode}): {errorContent}";
            return View(newEvent);
        }

        public async Task<IActionResult> ManageCategories(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var eventItem = await _apiService.GetAsync<EventDto>($"api/Events/{eventId}");
            if (eventItem == null) return NotFound();
            return View(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string name, string description)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var newCategory = new CategoryDto { EventId = eventId, Name = name, Description = description };
            var response = await _apiService.PostAsync("api/Categories", newCategory);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("ManageCategories", new { eventId = eventId });

            var error = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"API Error ({response.StatusCode}): {error}";
            
            return RedirectToAction("ManageCategories", new { eventId = eventId });
        }

        public async Task<IActionResult> ViewEnrolments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var eventItem = await _apiService.GetAsync<EventDto>($"api/Events/{eventId}");
            if (eventItem == null) return NotFound();

            return View(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(int enrolmentId, string finishTime, int position, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            if (!TimeSpan.TryParse(finishTime, out TimeSpan timeSpan))
            {
                TempData["Error"] = "Invalid time format. Use HH:mm:ss";
                return RedirectToAction("ViewEnrolments", new { eventId = eventId });
            }

            var resultDto = new ResultDto 
            { 
                EnrolmentId = enrolmentId, 
                FinishTime = timeSpan, 
                FinishingPosition = position 
            };

            var response = await _apiService.PostAsync($"api/Results/enrolment/{enrolmentId}", resultDto);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("ViewEnrolments", new { eventId = eventId });

            var error = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"API Error: {error}";
            return RedirectToAction("ViewEnrolments", new { eventId = eventId });
        }
    }
}