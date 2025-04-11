using FootballManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    public class ApiTeamWrapper
    {
        public ApiTeamModel team { get; set; }
        public ApiVenue venue { get; set; }
    }

    public class ApiTeamModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string country { get; set; }
        public string logo { get; set; }
        public string stadiumName { get; set; }
    }
    public class ApiVenue {
        public int id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public int capacity { get; set; }
        public string surface { get; set; }
        public string image { get; set; }
    }

    public class ApiFootballResponse
    {
        public List<ApiTeamWrapper> response { get; set; }
    }
}