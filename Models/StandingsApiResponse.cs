using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    public class StandingsApiResponse
    {
        public List<LeagueWrapper> response { get; set; }
    }

    public class LeagueWrapper
    {
        public LeagueData league { get; set; }
    }

    public class LeagueData
    {
        public List<List<StandingTeam>> standings { get; set; }
    }

    public class StandingTeam
    {
        public ApiTeam team { get; set; }
        public int points { get; set; }
        public MatchRecord all { get; set; }
    }

    public class ApiTeam
    {
        public int id { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
    }

    public class MatchRecord
    {
        public int played { get; set; }
        public int win { get; set; }
        public int draw { get; set; }
        public int lose { get; set; }
        public GoalData goals { get; set; }
    }

    public class GoalData
    {
        public int @for { get; set; }
        public int against { get; set; }
    }

    public class LeagueOption
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
    }
}