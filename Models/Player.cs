using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

        public int JerseyNumber { get; set; }

        public int? Age { get; set; } 

        [Required]
        public decimal Height { get; set; }

        [Required]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Team is required")]
        [ForeignKey("Team")]
        public int TeamId { get; set; }
        public Team Team { get; set; }

        [StringLength(100)]
        public string Nationality { get; set; }

        [StringLength(100)]
        public string TeamName { get; set; }

        [StringLength(100)]
        public string LeagueName { get; set; }

        public int TotalGoals { get; set; }

        public int TotalShots { get; set; }

        public int TotalPasses { get; set; }
    }
}
