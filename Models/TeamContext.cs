using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace FootballManager.Models
{
    public class TeamContext:DbContext
    {
        public TeamContext() : base("TeamDBConnection") { }
        public DbSet<Team> Teams { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<Staff> Staff { get; set; }

        public DbSet<Injury> Injuries { get; set; }

        public DbSet<Match> Matches { get; set; }

       public DbSet<Account> Accounts { get; set; }

    }
}