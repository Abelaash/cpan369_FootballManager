using System.Collections.Generic;

namespace FootballManager.Models
{
    public class ApiPlayerResponse
    {
        public List<ApiPlayerWrapper> response { get; set; }
        public ApiPaging paging { get; set; }
    }

    public class ApiPlayerWrapper
    {
        public ApiPlayer player { get; set; }
        public List<ApiPlayerStat> statistics { get; set; }
    }

    public class ApiPlayer
    {
        public int id { get; set; }
        public string name { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int? age { get; set; }
        public string nationality { get; set; }
        public string position { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string photo { get; set; }
        public ApiPlayerStat statistics { get; set; }
    }

    public class ApiPlayerStat
    {
        public ApiPlayerTeam team { get; set; }
        public ApiLeague league { get; set; }
        public ApiGameStats games { get; set; }
        public ApiShots shots { get; set; }
        public ApiGoals goals { get; set; }
        public ApiPasses passes { get; set; }
    }

    public class ApiPlayerTeam
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
    }

    public class ApiLeague
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string logo { get; set; }
        public string flag { get; set; }
        public int season { get; set; }
    }

    public class ApiGameStats
    {
        public int? appearences { get; set; }
        public int? lineups { get; set; }
        public int? minutes { get; set; }
        public string position { get; set; }
        public int? number { get; set; }
        public string rating { get; set; }
        public bool? captain { get; set; }
    }

    public class ApiShots
    {
        public int? total { get; set; }
        public int? on { get; set; }
    }

    public class ApiGoals
    {
        public int? total { get; set; }
        public int? assists { get; set; }
        public int? conceded { get; set; }
    }

    public class ApiPasses
    {
        public int? total { get; set; }
        public int? key { get; set; }
        public string accuracy { get; set; }
    }

    public class ApiPaging
    {
        public int current { get; set; }
        public int total { get; set; }
    }
}
