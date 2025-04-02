using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    [Table("Matchestbl")]
    public class Match
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("HomeTeam")]
        [Column("home_team_id")]  // ✅ Explicitly map to SQL column
        public int HomeTeamId { get; set; }

        [ForeignKey("AwayTeam")]
        [Column("away_team_id")]  // ✅ Explicitly map to SQL column
        public int AwayTeamId { get; set; }

        [Column("match_date")]  // ✅ Explicitly map to SQL column
        public DateTime MatchDate { get; set; }

        [NotMapped]
        public string HomeTeamLogo { get; set; }

        [NotMapped]
        public string AwayTeamLogo { get; set; }


        [NotMapped]
        public string HomeTeamName { get; set; }

        [NotMapped]
        public string AwayTeamName { get; set; }


        [Required]
        [StringLength(255)]
        [Column("venue")]  // ✅ Explicitly map to SQL column
        public string Venue { get; set; }

        [Required]
        [StringLength(20)]
        [Column("status")]  // ✅ Explicitly map to SQL column
        public string Status { get; set; } = "Upcoming";

        [Column("score_home")]  // ✅ Explicitly map to SQL column
        public int? ScoreHome { get; set; }

        [Column("score_away")]  // ✅ Explicitly map to SQL column
        public int? ScoreAway { get; set; }

        public virtual Team HomeTeam { get; set; }
        public virtual Team AwayTeam { get; set; }
    }
}