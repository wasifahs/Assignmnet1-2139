using Assignment1_2139.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment1_2139.Controllers
{
    [Authorize(Policy = "CanManageEvents")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AnalyticsController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> SalesByCategory()
        {
            var data = await _db.EventPurchases
                .Include(ep => ep.Event)
                    .ThenInclude(e => e.Category)
                .GroupBy(ep => ep.Event.Category.Name)
                .Select(g => new
                {
                    category = g.Key,
                    tickets = g.Sum(ep => ep.TicketQuantity)
                })
                .ToListAsync();

            return Json(data);
        }

        public async Task<IActionResult> RevenuePerMonth()
        {
            var data = await _db.Purchases
                .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    revenue = g.Sum(p => p.TotalCost)
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync();

            return Json(data);
        }

        public async Task<IActionResult> TopSelling()
        {
            var data = await _db.EventPurchases
                .Include(ep => ep.Event)
                .GroupBy(ep => new { ep.EventId, ep.Event.Title })
                .Select(g => new
                {
                    eventId = g.Key.EventId,
                    title = g.Key.Title,
                    sold = g.Sum(ep => ep.TicketQuantity)
                })
                .OrderByDescending(x => x.sold)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
    }
}
