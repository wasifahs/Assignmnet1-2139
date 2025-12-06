using Assignment1_2139.Data;
using Assignment1_2139.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Assignment1_2139.Controllers
{
    [Authorize(Roles = "Admin,Organizer")]
    public class EventPurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EventPurchasesController(ApplicationDbContext context) => _context = context;

        // GET: EventPurchases
        public async Task<IActionResult> Index()
        {
            var eventPurchases = await _context.EventPurchases
                .Include(ep => ep.Event)
                .Include(ep => ep.Purchase)
                .ToListAsync();
            return View(eventPurchases);
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewBag.Events = new SelectList(_context.Events, "Id", "Title");
            ViewBag.Purchases = new SelectList(_context.Purchases, "PurchaseId", "GuestEmail");
            return View();
        }

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventPurchase model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Events = new SelectList(_context.Events, "Id", "Title", model.EventId);
                ViewBag.Purchases = new SelectList(_context.Purchases, "PurchaseId", "GuestEmail", model.PurchaseId);
                return View(model);   // <-- MISSING LINE (CRITICAL)
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.EventPurchases.Add(model);
            await _context.SaveChangesAsync();

            await RecalculatePurchaseTotal(model.PurchaseId);

            return RedirectToAction(nameof(Index));
        }


        // GET: Edit
        public async Task<IActionResult> Edit(int eventId, int purchaseId)
        {
            var ep = await _context.EventPurchases
                .Include(e => e.Event)
                .Include(e => e.Purchase)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.PurchaseId == purchaseId);

            if (ep == null) return NotFound();

            ViewBag.Events = new SelectList(_context.Events, "Id", "Title", ep.EventId);
            ViewBag.Purchases = new SelectList(_context.Purchases, "PurchaseId", "GuestEmail", ep.PurchaseId);

            return View(ep);
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int eventId, int purchaseId, EventPurchase model)
        {
            if (eventId != model.EventId || purchaseId != model.PurchaseId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Events = new SelectList(_context.Events, "Id", "Title", model.EventId);
                ViewBag.Purchases = new SelectList(_context.Purchases, "PurchaseId", "GuestEmail", model.PurchaseId);

                return View(model);
            }

            var ep = await _context.EventPurchases
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.PurchaseId == purchaseId);

            if (ep == null) return NotFound();

            ep.TicketQuantity = model.TicketQuantity;
            ep.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await RecalculatePurchaseTotal(ep.PurchaseId);

            Log.Information("EventPurchase edited by {User} for Event {EventId}", User.Identity?.Name, model.EventId);
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int eventId, int purchaseId)
        {
            var ep = await _context.EventPurchases
                .Include(e => e.Event)
                .Include(e => e.Purchase)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.PurchaseId == purchaseId);

            if (ep == null) return NotFound();
            return View(ep);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int eventId, int purchaseId)
        {
            var ep = await _context.EventPurchases
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.PurchaseId == purchaseId);

            if (ep != null)
            {
                int purchaseIdRef = ep.PurchaseId;
                _context.EventPurchases.Remove(ep);
                await _context.SaveChangesAsync();

                await RecalculatePurchaseTotal(purchaseIdRef);

                Log.Information("EventPurchase deleted by {User} for Event {EventId}", User.Identity?.Name, eventId);
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method: Recalculate Purchase Total
        private async Task RecalculatePurchaseTotal(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.EventPurchases)
                .ThenInclude(ep => ep.Event)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

            if (purchase != null)
            {
                purchase.TotalCost = purchase.EventPurchases?.Sum(ep => (ep.Event?.TicketPrice ?? 0) * ep.TicketQuantity) ?? 0;
                purchase.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}







