using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    public class ApiPlayerWrapper
    {
        public ApiPlayer player { get; set; }
    }

    public class ApiPlayer
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public string position { get; set; }
        public int? number { get; set; }
        public List<ApiPlayerStat> statistics { get; set; }
    }

    public class ApiPlayerStat
    {
        public string height { get; set; }
        public string weight { get; set; }
    }

    public class ApiPlayerResponse
    {
        public List<ApiPlayerWrapper> response { get; set; }
    }
}
