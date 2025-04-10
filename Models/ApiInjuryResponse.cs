using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    public class ApiInjuryResponse
    {
        public List<ApiInjury> Response { get; set; }
    }

    public class ApiInjury
    {
        public ApiPlayerInfo Player { get; set; }
        public ApiTeamInfo Team { get; set; }
        public string Reason { get; set; }
        public string Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class ApiPlayerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ApiTeamInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
    }
}
