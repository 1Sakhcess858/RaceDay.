using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.DTOs;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("UserRole") == "Participant";
        }

        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var enrolments = await _apiService.GetAsync<List<EnrolmentDto>>("api/Enrolments/my");
            return View(enrolments ?? new List<EnrolmentDto>());
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var eventItem = await _apiService.GetAsync<EventDto>($"api/Events/{id}");
            if (eventItem == null) return NotFound();

            return View(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var response = await _apiService.PostAsync($"api/Enrolments/event/{eventId}/enrol", categoryId);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("MyEvents");
            }

            var error = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Failed to enrol: {error}";
            
            // Re-fetch event to show details again
            var eventItem = await _apiService.GetAsync<EventDto>($"api/Events/{eventId}");
            return View("EventDetails", eventItem);
        }
    }
}