using System.Diagnostics;
using Assignment1_2139.Data;
using Microsoft.AspNetCore.Mvc;
using Assignment1_2139.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore; // Needed for Include
using Serilog;

namespace Assignment1_2139.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            var events = await _db.Events
                .Include(e => e.Category)
                .ToListAsync();

            return View(events); // pass events to the view
        }

        // Optional: About page
        public IActionResult About()
        {
            ViewData["Message"] = "Virtual Event Ticketing System - ASP.NET Core MVC Project";
            return View();
        }

        // Optional: Error page
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        public IActionResult Error500()
        {
            Response.StatusCode = 500;
            var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            Log.Error(ex, "Unhandled exception");
            return View("ServerError");
        }

        // -------------------------
        // Updates Start Here
        // -------------------------

        // General search
        [HttpGet]
        public IActionResult GeneralSearch(string searchType, string searchString)
        {
            searchType = searchType?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchType) || string.IsNullOrWhiteSpace(searchString))
                return RedirectToAction(nameof(Index));

            if (searchType == "events")
            {
                var results = _db.Events
                    .Include(e => e.Category)
                    .Where(e => e.Title.Contains(searchString))
                    .ToList();

                return View("Index", results);
            }
            else if (searchType == "tickets")
            {
                var results = _db.Purchases
                    .Include(p => p.EventPurchases)
                    .Where(p => p.GuestEmail.Contains(searchString))
                    .ToList();

                return View("TicketSearchResults", results);
            }

            return RedirectToAction(nameof(Index));
        }

        // Live AJAX search
        [HttpGet]
        public IActionResult LiveSearch(string searchType, string searchString)
        {
            searchType = searchType?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchType) || string.IsNullOrWhiteSpace(searchString))
                return Content("");

            if (searchType == "events")
            {
                var events = _db.Events
                    .Include(e => e.Category)
                    .Where(e => e.Title.Contains(searchString))
                    .Take(5)
                    .ToList();

                return PartialView("_EventPartial", events);
            }
            else if (searchType == "tickets")
            {
                var tickets = _db.Purchases
                    .Include(p => p.EventPurchases)
                    .Where(p => p.GuestEmail.Contains(searchString))
                    .Take(5)
                    .ToList();

                return PartialView("_TicketPartial", tickets);
            }

            return Content("");
        }

        // -------------------------
        // Updates End Here
        // -------------------------
    }
}
