using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Assignment1_2139.Models; // Correct namespace for Event

namespace Assignment1_2139.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        // Navigation property: A category can have multiple events
        public virtual ICollection<Event>? Events { get; set; } = new List<Event>();
    }
}