using System.Collections.Generic;

namespace Assignment1_2139.Models.ViewModels
{
    public class DashboardViewModel
    {
        public ApplicationUser User { get; set; }

        // Attendee
        public IEnumerable<Purchase> UpcomingPurchases { get; set; }
        public IEnumerable<Purchase> HistoryPurchases { get; set; }

        // Organizer/Admin
        public IEnumerable<Event> MyEvents { get; set; }
        public decimal TotalRevenue { get; set; }

        // Analytics
        public object TicketSalesByCategory { get; set; }
        public object RevenuePerMonth { get; set; }
        public object Top5Events { get; set; }
    }
}