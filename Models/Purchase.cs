using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment1_2139.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Required, StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string GuestEmail { get; set; } = string.Empty;

        public decimal TotalCost { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
       

        public ICollection<EventPurchase> EventPurchases { get; set; } = new List<EventPurchase>();

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }  // Nullable DateTime
        public string? UserId { get; set; }


        // CreatedBy property
        public string? CreatedBy { get; set; } = "System";

        // Remove the previous method or code block with NotImplementedException
    }
}