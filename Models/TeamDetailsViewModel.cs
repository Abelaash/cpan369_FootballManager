using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    public class TeamDetailsViewModel
    {
        public Team Team { get; set; }
        public List<Match> UpcomingMatches { get; set; }
        public List<Player> Squad { get; set; }
    }
}