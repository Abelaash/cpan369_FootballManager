using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    [Table("Playertbl")]
    public class Player
    {
        [Key]
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; }

        [Required]
        [Range(1, 99, ErrorMessage = "Jersey Number must be between 1 and 99")]
        public int JerseyNumber { get; set; }

        [Required]
        [Range(16, 45, ErrorMessage = "Age must be between 16 and 45")]
        public int Age { get; set; }

        [Required]
        [Range(1.5, 2.5)]
        public decimal Height { get; set; }



        [Required]
        [Range(50, 120, ErrorMessage = "Weight must be between 50kg and 120kg")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Team is required")]
        [ForeignKey("Team")]
        public int TeamId { get; set; }
        public Team Team { get; set; } // object of foreign key model Team

    }
}