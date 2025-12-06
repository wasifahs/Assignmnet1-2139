using Assignment1_2139.Data;
using Assignment1_2139.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Assignment1_2139;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Assignment1_2139.Helpers;
using Newtonsoft.Json;

namespace Assignment1_2139.Controllers
{
    public class EventsController : Controller

    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ===============================
        // INDEX (Filters)
        // ===============================
        public async Task<IActionResult> Index(
            string? searchTitle,
            int? categoryId,
            DateTime? startDate,
            DateTime? endDate,
            string? ticketStatus,
            Event.Status? status,
            string? sortOrder)
        {
            var events = _context.Events.Include(e => e.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
                events = events.Where(e => e.Title.Contains(searchTitle));

            if (categoryId.HasValue)
                events = events.Where(e => e.CategoryId == categoryId.Value);

            if (startDate.HasValue)
                events = events.Where(e => e.StartDate >= DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc));

            if (endDate.HasValue)
                events = events.Where(e => e.EndDate <= DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc));

            if (!string.IsNullOrEmpty(ticketStatus))
            {
                if (ticketStatus == "available")
                    events = events.Where(e => e.AvailableTickets > 0);
                else if (ticketStatus == "soldout")
                    events = events.Where(e => e.AvailableTickets <= 0);
            }

            if (status.HasValue)
                events = events.Where(e => e.EventStatus == status.Value);

            // Sorting
            events = sortOrder switch
            {
                "title_asc" => events.OrderBy(e => e.Title),
                "title_desc" => events.OrderByDescending(e => e.Title),
                "date_asc" => events.OrderBy(e => e.StartDate),
                "date_desc" => events.OrderByDescending(e => e.StartDate),
                "price_asc" => events.OrderBy(e => e.TicketPrice),
                "price_desc" => events.OrderByDescending(e => e.TicketPrice),
                _ => events.OrderBy(e => e.Title)
            };

            // Populate dropdowns
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["AvailabilityList"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "available", Text = "Available" },
                new SelectListItem { Value = "soldout", Text = "Sold Out" }
            };
            ViewData["StatusList"] = Enum.GetValues(typeof(Event.Status))
                .Cast<Event.Status>()
                .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() });

            return View(await events.ToListAsync());
        }

        // ===============================
        // CREATE (GET)
        // ===============================
        public IActionResult Create()
        {
            ViewData["StatusList"] = Enum.GetValues(typeof(Event.Status))
                .Cast<Event.Status>()
                .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                .ToList();

            var categories = _context.Categories.ToList();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name");

            return View();
        }

        // ===============================
        // CREATE (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["StatusList"] = Enum.GetValues(typeof(Event.Status))
                    .Cast<Event.Status>()
                    .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                    .ToList();

                ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name", model.CategoryId);
                return View(model);
            }

            model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
            model.EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);
            model.CreatedAt = DateTime.UtcNow;

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("Organizer") || User.IsInRole("Admin"))
            {
                model.CreatedByUserId = userId ?? "Guest"; // Assign Organizer/Admin
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ===============================
        // EDIT (GET & POST)
        // ===============================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ViewData["StatusList"] = Enum.GetValues(typeof(Event.Status))
                .Cast<Event.Status>()
                .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                .ToList();

            var categories = _context.Categories?.ToList() ?? new List<Category>();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name", ev.CategoryId);

            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event updatedEvent)
        {
            if (id != updatedEvent.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewData["StatusList"] = Enum.GetValues(typeof(Event.Status))
                    .Cast<Event.Status>()
                    .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                    .ToList();

                var categories = _context.Categories?.ToList() ?? new List<Category>();
                ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name", updatedEvent.CategoryId);
                return View(updatedEvent);
            }

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            existingEvent.Title = updatedEvent.Title;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.StartDate = DateTime.SpecifyKind(updatedEvent.StartDate, DateTimeKind.Utc);
            existingEvent.EndDate = DateTime.SpecifyKind(updatedEvent.EndDate, DateTimeKind.Utc);
            existingEvent.CategoryId = updatedEvent.CategoryId;
            existingEvent.TicketPrice = updatedEvent.TicketPrice;
            existingEvent.AvailableTickets = updatedEvent.AvailableTickets;
            existingEvent.EventStatus = updatedEvent.EventStatus;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DETAILS
        // ===============================
        public async Task<IActionResult> Details(int id)
        {
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.EventPurchases)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null) return NotFound();

            return View(eventModel);
        }

        // ===============================
        // DELETE (GET & POST)
        // ===============================
        public async Task<IActionResult> Delete(int id)
        {
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null) return NotFound();

            return View(eventModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel != null)
            {
                var relatedPurchases = _context.EventPurchases.Where(ep => ep.EventId == id);
                _context.EventPurchases.RemoveRange(relatedPurchases);
                _context.Events.Remove(eventModel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // AJAX: Live search
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var events = string.IsNullOrEmpty(searchTerm)
                ? await _context.Events.ToListAsync()
                : await _context.Events
                    .Where(e => e.Title.Contains(searchTerm))
                    .ToListAsync();

            return PartialView("_EventPartial", events);
        }

// POST: /Events/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int eventId, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            var ev = _context.Events.Find(eventId);
            if(ev == null) return Json(new { success = false, message="Event not found" });

            var existing = cart.FirstOrDefault(c => c.EventId == eventId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem { EventId = eventId, Quantity = quantity, Event = ev });

            HttpContext.Session.SetObject("Cart", cart);

            int totalQuantity = cart.Sum(c => c.Quantity);
            decimal cartTotal = cart.Sum(c => c.Event.TicketPrice * c.Quantity);

            return Json(new { success = true, totalQuantity, cartTotal });
        }



        [HttpPost]
        public IActionResult UpdateCartQuantity(int eventId, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.EventId == eventId);
            if (item != null) item.Quantity = quantity;

            HttpContext.Session.SetObject("Cart", cart);

            int totalQuantity = cart.Sum(c => c.Quantity);
            decimal cartTotal = cart.Sum(c => c.Event.TicketPrice * c.Quantity);
            decimal itemTotal = item != null ? item.Event.TicketPrice * item.Quantity : 0;

            return Json(new { success = true, itemTotal, cartTotal, totalQuantity });
        }





        // ===============================
        // ORGANIZER/ADMIN ANALYTICS
        // ===============================
        [Authorize(Roles = "Organizer,Admin")]
        public IActionResult MyAnalytics() => View();

        [Authorize(Roles = "Organizer,Admin")]
        public IActionResult SalesData()
        {
            var data = _context.Events
                .Select(e => new
                {
                    e.Title,
                    TicketsSold = e.EventPurchases.Sum(ep => ep.TicketQuantity),
                    Revenue = e.EventPurchases.Sum(ep => ep.TicketQuantity * ep.Event.TicketPrice)
                });

            return Json(data);
        }


        // GET: Events/Buy/3
        [HttpGet]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> Buy(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            return View(ev);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Attendee")]
        public async Task<IActionResult> Buy(int eventId, int quantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return Json(new { success = false, message = "Event not found" });

            if (quantity <= 0 || quantity > ev.AvailableTickets)
                return Json(new { success = false, message = "Invalid quantity" });

            var purchase = new Purchase
            {
                UserId = userId,
                GuestName = "N/A",
                GuestEmail = "none@none.com",
                TotalCost = ev.TicketPrice * quantity,
                PurchaseDate = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            var eventPurchase = new EventPurchase
            {
                EventId = eventId,
                PurchaseId = purchase.PurchaseId,
                TicketQuantity = quantity,
                PurchaseDate = DateTime.UtcNow
            };
            _context.EventPurchases.Add(eventPurchase);

            ev.AvailableTickets -= quantity;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Purchase successful!" });
        }
    }
}












