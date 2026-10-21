using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.DTOs;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _apiService.GetAsync<List<EventDto>>("api/Events");

            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View(events ?? new List<EventDto>());
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}