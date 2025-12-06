using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment1_2139.Models
{
    public class Event
    {
        public enum Status
        {
            New,
            InProgress,
            Completed
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal TicketPrice { get; set; }

        [Required]
        public int AvailableTickets { get; set; }

        [Required]
        public Status EventStatus { get; set; } = Status.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        
        
        // Relationship: purchases
        public virtual ICollection<EventPurchase>? EventPurchases { get; set; } = new List<EventPurchase>();
    }
}