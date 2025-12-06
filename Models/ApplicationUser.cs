using Microsoft.AspNetCore.Identity;
using System;

namespace Assignment1_2139.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty; // Required, initialize with empty string
        public string PhoneNumber { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } 
        public DateTime DateOfBirth { get; set; } // must be non-nullable


    }

}   
    