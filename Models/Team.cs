using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    [Table("Teamtbl")]
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters")]
        public string City { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Wins must be a non-negative number")]
        public int Wins { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Losses must be a non-negative number")]
        public int Losses { get; set; } = 0;

        [Required]
        [StringLength(100)]
        public string League { get; set; }


        [Range(0, int.MaxValue, ErrorMessage = "Draws must be a non-negative number")]
        public int Draws { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Matches Played must be a non-negative number")]
        public int matches_played { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Goals For must be a non-negative number")]
        public int goals_for { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Goals Against must be a non-negative number")]
        public int goals_against { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Points must be a non-negative number")]
        public int Points { get; set; } = 0;

        public string LogoUrl { get; set; }

        public virtual ICollection<Player> Players { get; set; }

    }
}
