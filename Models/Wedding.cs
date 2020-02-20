using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace WeddingPlanner.Models
{
    public class Wedding
    {

        [Key]
        public int WeddingId { get; set; }
        [Required]
        [MinLength(4)]
        public string WedderOne { get; set; }
        [Required]
        [MinLength(4)]
        public string WedderTwo { get; set; }
         [Required]
        public DateTime WeddingDate { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int UserId {get;set;}
        public User creator {get;set;}
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<RSVP> Guests { get; set; }

    }
}