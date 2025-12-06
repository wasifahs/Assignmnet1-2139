using System;
using System.ComponentModel.DataAnnotations;

namespace Assignment1_2139.Models
{
    public class EventPurchase
    { 
        public int EventId { get; set; }
        public Event? Event { get; set; } 


        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        [Range(1, 100, ErrorMessage = "Ticket quantity must be at least 1")]
        public int TicketQuantity { get; set; }

        [StringLength(256)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? Rating { get; set; }
        
    }
}