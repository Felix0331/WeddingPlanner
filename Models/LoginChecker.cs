using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace WeddingPlanner.Models
{
    public class LoginChecker
    {
        [Required]
        [EmailAddress]
        public string LoginEmail { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
        public string LoginPassword { get; set; }

    }
}