using Assignment1_2139.Data;
using Assignment1_2139.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Assignment1_2139.Controllers
{
    [Authorize(Roles = "Admin,Organizer")]
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Purchases
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .ToListAsync();
            return View(purchases);
        }

        // GET: Details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

            if (purchase == null) return NotFound();
            return View(purchase);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Events = _context.Events.ToList();
            return View(new Purchase());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Purchase model)
        {
            ViewBag.Events = _context.Events.ToList();

            // Keep only valid selections
            model.EventPurchases = model.EventPurchases
                .Where(ep => ep.EventId > 0)
                .ToList();

            if (!model.EventPurchases.Any())
            {
                ModelState.AddModelError("", "Please select at least one event.");
                return View(model);
            }

            model.TotalCost = model.EventPurchases.Sum(ep =>
            {
                var ev = _context.Events.FirstOrDefault(e => e.Id == ep.EventId);
                return ev != null ? ev.TicketPrice * ep.TicketQuantity : 0;
            });

            model.CreatedAt = DateTime.UtcNow;
            model.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            _context.Purchases.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

            if (purchase == null)
                return NotFound();

            ViewBag.EventList = await _context.Events
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
                })
                .ToListAsync();

            return View(purchase);
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int purchaseId, Purchase model)
        {
            ViewBag.EventList = await _context.Events
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
                })
                .ToListAsync();

            if (purchaseId != model.PurchaseId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var purchase = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

            if (purchase == null)
                return NotFound();

            purchase.GuestName = model.GuestName;
            purchase.GuestEmail = model.GuestEmail;
            purchase.UpdatedAt = DateTime.UtcNow;

            purchase.TotalCost = purchase.EventPurchases?
                .Sum(ep => (ep.Event?.TicketPrice ?? 0) * ep.TicketQuantity) ?? 0;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Delete(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

            if (purchase == null)
                return NotFound();

            return View(purchase);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> DeleteConfirmed(int purchaseId)
        {
            var purchase = await _context.Purchases.FindAsync(purchaseId);
            if (purchase == null)
                return NotFound();

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: RateEvent (AJAX)
        [HttpPost]
        public async Task<IActionResult> RateEvent([FromBody] EventRatingDto dto)
        {
            if (dto == null || dto.Rating < 1 || dto.Rating > 5)
                return Json(new { success = false });

            var ep = await _context.EventPurchases
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EventId == dto.EventId && e.PurchaseId == dto.PurchaseId);

            if (ep == null) return Json(new { success = false });

            ep.Rating = dto.Rating;
            ep.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ✅ Download ticket PDF
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadTicketPdf(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
                return NotFound();

            var content = $"Ticket for {purchase.GuestName}\n\nEvents:\n";
            foreach (var ep in purchase.EventPurchases)
            {
                content += $"{ep.Event.Title} - Tickets: {ep.TicketQuantity}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(bytes, "application/pdf", $"Ticket_{purchase.PurchaseId}.pdf");
        }
    }

    // DTO for AJAX Rating
    public class EventRatingDto
    {
        public int PurchaseId { get; set; }
        public int EventId { get; set; }
        public int Rating { get; set; }
    }
}








