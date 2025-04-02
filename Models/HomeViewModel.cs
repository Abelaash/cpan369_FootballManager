using System;
using System.Collections.Generic;

namespace FootballManager.Models
{
    public class HomeViewModel
    {
        public List<Team> Standings { get; set; }
        public List<Match> UpcomingMatches { get; set; }

        // Display league title dynamically in the view
        public string SelectedLeague { get; set; }
    }
}
