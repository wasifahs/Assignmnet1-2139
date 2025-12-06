using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Assignment1_2139.Models;

namespace Assignment1_2139.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        // Your DbSets
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<EventPurchase> EventPurchases { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } // <-- add this

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important for Identity

            modelBuilder.Entity<EventPurchase>()
                .HasKey(ep => new { ep.EventId, ep.PurchaseId });

            modelBuilder.Entity<EventPurchase>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.EventPurchases)
                .HasForeignKey(ep => ep.EventId);

            modelBuilder.Entity<EventPurchase>()
                .HasOne(ep => ep.Purchase)
                .WithMany(p => p.EventPurchases)
                .HasForeignKey(ep => ep.PurchaseId);
        }
    }
}