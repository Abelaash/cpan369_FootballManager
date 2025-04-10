using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    public class ApiCoachResponse
    {
        public List<ApiCoachWrapper> response { get; set; }
    }

    public class ApiCoachWrapper
    {
        public int id { get; set; }
        public string name { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string type { get; set; }
        public ApiCoachTeam team { get; set; }
    }


    public class ApiCoachTeam
    {
        public int id { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
    }

    public class ApiCoachDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Team { get; set; }
    }
}

