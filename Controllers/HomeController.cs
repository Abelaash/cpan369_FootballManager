using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private TeamContext db = new TeamContext();

        public ActionResult Index()
        {
            var teams = db.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.goals_for)
                .ToList();

            var groupedLeagues = teams
                .GroupBy(t => t.League)
                .ToList();

            var upcomingMatches = db.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.MatchDate > DateTime.Now)
                .OrderBy(m => m.MatchDate)
                .Take(5)
                .ToList();

            var viewModel = new HomeViewModel
            {
                Standings = teams,
                UpcomingMatches = upcomingMatches
            };

            ViewBag.GroupedLeagues = groupedLeagues;
            ViewBag.LeagueNames = groupedLeagues.Select(g => g.Key).ToList();

            return View(viewModel);
        }

        public ActionResult Teams()
        {
            ViewBag.Message = "View list of Teams.";
            return View("~/Views/Teams/Index.cshtml");
        }

        public ActionResult Players()
        {
            ViewBag.Message = "View list of Players.";
            return View("~/Views/Players/Index.cshtml");
        }

        public ActionResult LeagueStandings()
        {
            var teamsByLeague = db.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.goals_for)
                .GroupBy(t => t.League)
                .ToList();

            return View(teamsByLeague);
        }

        // get league details for a specific league
        public ActionResult LeagueDetails(string league)
        {
            List<Team> leagueTeams;
            List<Match> upcoming;

            var apiLeagues = new List<string> { "Premier League", "La Liga", "Bundesliga", "Ligue 1", "Serie A", "Eredivisie" };

            var apiService = new ApiFootballService();

            leagueTeams = db.Teams
                    .Where(t => t.League.ToLower() == league.ToLower())
                    .OrderByDescending(t => t.Points)
                    .ThenByDescending(t => t.goals_for)
                    .ToList();
            upcoming = apiService.GetUpcomingMatches(league);

            var model = new HomeViewModel
            {
                Standings = leagueTeams,
                UpcomingMatches = upcoming,
                SelectedLeague = league
            };

            return View("LeagueDetails", model);
        }

        public ActionResult Staff()
        {
            ViewBag.Message = "View list of Staff.";
            return View("~/Views/Staffs/Index.cshtml");
        }

        // search suggestions for players and teams
        [HttpGet]
        public JsonResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { results = new List<string>() }, JsonRequestBehavior.AllowGet);
            }

            // Search for Players (FirstName + LastName)
            var playerResults = db.Players
                .Where(p => (p.FirstName + " " + p.LastName).Contains(query))
                .Select(p => new {
                    Id = p.PlayerId, 
                    Name = p.FirstName + " " + p.LastName,
                    Type = "Player"
                })
                .Take(5)
                .ToList();

            // Search for Teams
            var teamResults = db.Teams
                .Where(t => t.Name.Contains(query))
                .Select(t => new {
                    Id = t.TeamId, 
                    Name = t.Name,
                    Type = "Team"
                })
                .Take(5)
                .ToList();

            // Combine and return results
            var results = playerResults.Concat(teamResults).ToList();

            return Json(new { results }, JsonRequestBehavior.AllowGet);
        }


    }
}
