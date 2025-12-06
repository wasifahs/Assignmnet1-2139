using Assignment1_2139.Data;
using Assignment1_2139.Models;
using Assignment1_2139.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment1_2139.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            // Attendee data: upcoming & past purchases
            var upcoming = await _db.Purchases
                .Include(p => p.EventPurchases)
                    .ThenInclude(ep => ep.Event)
                .Where(p => p.UserId == user.Id && p.EventPurchases.Any(ep => ep.Event.StartDate >= DateTime.UtcNow))
                .ToListAsync();

            var history = await _db.Purchases
                .Include(p => p.EventPurchases)
                    .ThenInclude(ep => ep.Event)
                .Where(p => p.UserId == user.Id && p.EventPurchases.Any(ep => ep.Event.StartDate < DateTime.UtcNow))
                .ToListAsync();

            // Organizer/Admin data
            var myEvents = Enumerable.Empty<Event>();
            decimal totalRevenue = 0;
            object ticketSalesByCategory = null;
            object revenuePerMonth = null;
            object top5Events = null;

            if (roles.Contains("Organizer"))
            {
                // Organizer: their own events
                myEvents = await _db.Events.Where(e => e.CreatedByUserId == user.Id).ToListAsync();

                totalRevenue = await _db.EventPurchases
                    .Include(ep => ep.Event)
                    .Where(ep => myEvents.Select(me => me.Id).Contains(ep.EventId))
                    .SumAsync(ep => ep.Event.TicketPrice * ep.TicketQuantity);
            }

            if (roles.Contains("Admin"))
            {
                // Admin: all events & total revenue
                myEvents = await _db.Events.ToListAsync();
                totalRevenue = await _db.Purchases.SumAsync(p => p.TotalCost);
            }

            if (roles.Contains("Organizer") || roles.Contains("Admin"))
            {
                // Analytics: Ticket sales by category
                ticketSalesByCategory = await _db.EventPurchases
                    .Include(ep => ep.Event)
                        .ThenInclude(e => e.Category)
                    .GroupBy(ep => ep.Event.Category.Name)
                    .Select(g => new { Category = g.Key, TicketsSold = g.Sum(ep => ep.TicketQuantity) })
                    .ToListAsync();

                // Revenue per month
                // Revenue per month
                revenuePerMonth = await _db.Purchases
                    .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
                    .Select(g => new {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(p => p.TotalCost)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();


                // Top 5 best-selling events
                top5Events = await _db.EventPurchases
                    .Include(ep => ep.Event)
                    .GroupBy(ep => new { ep.EventId, ep.Event.Title })
                    .Select(g => new { Title = g.Key.Title, Sold = g.Sum(ep => ep.TicketQuantity) })
                    .OrderByDescending(x => x.Sold)
                    .Take(5)
                    .ToListAsync();
            }

            var model = new DashboardViewModel
            {
                User = user,
                UpcomingPurchases = upcoming,
                HistoryPurchases = history,
                MyEvents = myEvents,
                TotalRevenue = totalRevenue,
                TicketSalesByCategory = ticketSalesByCategory,
                RevenuePerMonth = revenuePerMonth,
                Top5Events = top5Events
            };

            // Render role-specific view
            if (roles.Contains("Admin"))
                return View("AdminDashboard", model);
            if (roles.Contains("Organizer"))
                return View("OrganizerDashboard", model);

            return View("AttendeeDashboard", model);
        }
    }
}













