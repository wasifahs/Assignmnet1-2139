using Microsoft.AspNetCore.Mvc;
using Assignment1_2139.Models;
using QRCoder;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Assignment1_2139.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public class TicketsController : Controller

{
    private readonly ApplicationDbContext _context;

    public TicketsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [Authorize(Roles = "Attendee")]
    public async Task<IActionResult> Buy(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return NotFound();

        // logged-in user
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // check if the user purchased this event
        bool hasPurchased = await _context.EventPurchases
            .AnyAsync(ep => ep.EventId == id && ep.Purchase.UserId == userId);

        ViewBag.AlreadyPurchased = hasPurchased;

        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Attendee")]
    public async Task<IActionResult> Buy(int eventId, int quantity)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null) return NotFound();

        if (quantity <= 0 || quantity > ev.AvailableTickets)
        {
            ModelState.AddModelError("", "Invalid quantity.");
            return View(ev);
        }

        // Purchase object
        var purchase = new Purchase
        {
            UserId = userId,
            GuestName = "N/A",       // REQUIRED BY MODEL
            GuestEmail = "none@none.com", // REQUIRED BY MODEL
            TotalCost = ev.TicketPrice * quantity,
            PurchaseDate = DateTime.UtcNow
        };

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();  // generates PurchaseId

        // Link event → purchase
        var eventPurchase = new EventPurchase
        {
            EventId = eventId,
            PurchaseId = purchase.PurchaseId, // CORRECT PROPERTY
            TicketQuantity = quantity,
            PurchaseDate = DateTime.UtcNow
        };

        _context.EventPurchases.Add(eventPurchase);

        // reduce available tickets
        ev.AvailableTickets -= quantity;

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Dashboard");
    }



    // Generate QR code for a purchase
    public async Task<IActionResult> GenerateQRCode(int id)
    {
        var purchase = await _context.EventPurchases
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.EventId == id);

        if (purchase == null) return NotFound();

        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode($"{purchase.EventId}-{purchase.Event.Title}", QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(20);

        return File(qrBytes, "image/png");
    }

    // Download ticket as PDF
    public async Task<IActionResult> DownloadPDF(int id)
    {
        var purchase = await _context.EventPurchases
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.EventId == id);

        if (purchase == null) return NotFound();

        using var ms = new MemoryStream();
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
            document.Add(new Paragraph($"Event: {purchase.Event.Title}"));
            document.Add(new Paragraph($"Start Date: {purchase.Event.StartDate:f}"));
            document.Add(new Paragraph($"Quantity: {purchase.TicketQuantity}"));
          
        }

        return File(ms.ToArray(), "application/pdf", $"Ticket_{purchase.EventId}.pdf");
    }
    
    [Authorize(Roles = "Attendee")]
    [HttpPost]
    public async Task<IActionResult> CancelTicket(int purchaseId, int eventId)
    {
        Console.WriteLine($"CancelTicket called: purchaseId={purchaseId}, eventId={eventId}");

        // Find the event purchase
        var ep = await _context.EventPurchases
            .Include(e => e.Purchase)
            .FirstOrDefaultAsync(e => e.PurchaseId == purchaseId && e.EventId == eventId);

        if (ep == null) return NotFound();

        _context.EventPurchases.Remove(ep);

        // Optional: remove purchase if no events remain
        var remaining = await _context.EventPurchases
            .Where(e => e.PurchaseId == purchaseId)
            .ToListAsync();
        if (!remaining.Any())
        {
            var purchase = await _context.Purchases.FindAsync(purchaseId);
            if (purchase != null) _context.Purchases.Remove(purchase);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Dashboard");
    }



}