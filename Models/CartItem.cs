using System;

namespace Assignment1_2139.Models
{
    public class CartItem
    {
        public int EventId { get; set; }
        public int Quantity { get; set; }

        // store event details in memory
        public Event Event { get; set; }
    }
}