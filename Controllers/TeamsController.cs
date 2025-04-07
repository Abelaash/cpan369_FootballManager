using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;
using Newtonsoft.Json;

namespace FootballManager.Controllers
{
    public class TeamsController : Controller
    {
        private TeamContext db = new TeamContext();

        // GET: Teams
        [Authorize]
        public ActionResult Index()
        {
            return View(db.Teams.ToList());
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var team = db.Teams.Find(id);
            if (team == null)
                return HttpNotFound();

            var upcomingMatches = db.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Status == "Upcoming" &&
                       (m.HomeTeamId == team.TeamId || m.AwayTeamId == team.TeamId))
                .OrderBy(m => m.MatchDate)
                .ToList();

            var squad = db.Players.Where(p => p.TeamId == team.TeamId).ToList();

            var viewModel = new TeamDetailsViewModel
            {
                Team = team,
                UpcomingMatches = upcomingMatches,
                Squad = squad
            };

            return View(viewModel);
        }


        public async Task<ActionResult> ImportFixturesFromApi()
        {
            var service = new ApiFootballService();
            var leagues = new List<string> { "Premier League", "La Liga", "Serie A", "Bundesliga", "Ligue 1", "Eredivisie" };

            foreach (var league in leagues)
            {
                var fixtures = service.GetUpcomingMatches(league);

                foreach (var fixture in fixtures)
                {
                    var homeTeam = db.Teams.FirstOrDefault(t => t.Name == fixture.HomeTeamName);
                    var awayTeam = db.Teams.FirstOrDefault(t => t.Name == fixture.AwayTeamName);

                    if (homeTeam != null && awayTeam != null && !db.Matches.Any(m =>
                        m.HomeTeamId == homeTeam.TeamId &&
                        m.AwayTeamId == awayTeam.TeamId &&
                        m.MatchDate == fixture.MatchDate))
                    {
                        fixture.HomeTeamId = homeTeam.TeamId;
                        fixture.AwayTeamId = awayTeam.TeamId;
                        fixture.Status = "Upcoming";
                        db.Matches.Add(fixture);
                    }
                }
            }

            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Fixtures imported successfully!";
            return RedirectToAction("Index");
        }








        // GET: Teams/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,City,Wins,Losses,Draws")] Team team)
        {
            if (ModelState.IsValid)
            {
                db.Teams.Add(team);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(team);
        }

        [Authorize(Roles = "Admin")]
        // GET: Teams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Team team = db.Teams.Find(id);
            if (team == null)
                return HttpNotFound();

            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TeamId,Name,City,Wins,Losses,Draws")] Team team)
        {
            if (ModelState.IsValid)
            {
                db.Entry(team).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(team);
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Team team = db.Teams.Find(id);
            if (team == null)
                return HttpNotFound();

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Team team = db.Teams.Find(id);
            db.Teams.Remove(team);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ✅ Import Teams from API
        public async Task<ActionResult> ImportFromApi()
        {
            var service = new ApiFootballService();

            var leagues = new List<string>
            {
                "Premier League",
                "La Liga",
                "Bundesliga",
                "Ligue 1",
                "Serie A",
                "Eredivisie"
            };

            var importedTeamNames = new HashSet<string>(db.Teams.Select(t => t.Name.ToLower()));

            foreach (var leagueName in leagues)
            {
                var apiTeams = service.GetLeagueStandings(leagueName);

                foreach (var apiTeam in apiTeams)
                {
                    var existingTeam = db.Teams.FirstOrDefault(t => t.Name.ToLower() == apiTeam.Name.ToLower());

                    if (existingTeam != null)
                    {
                        existingTeam.Wins = apiTeam.Wins;
                        existingTeam.Losses = apiTeam.Losses;
                        existingTeam.Draws = apiTeam.Draws;
                        existingTeam.matches_played = apiTeam.matches_played;
                        existingTeam.goals_for = apiTeam.goals_for;
                        existingTeam.goals_against = apiTeam.goals_against;
                        existingTeam.Points = apiTeam.Points;
                        existingTeam.LogoUrl = apiTeam.LogoUrl;
                        existingTeam.League = apiTeam.League;
                    }
                    else
                    {
                        db.Teams.Add(new Team
                        {
                            Name = apiTeam.Name,
                            City = apiTeam.City ?? "Unknown",
                            Wins = apiTeam.Wins,
                            Losses = apiTeam.Losses,
                            Draws = apiTeam.Draws,
                            matches_played = apiTeam.matches_played,
                            goals_for = apiTeam.goals_for,
                            goals_against = apiTeam.goals_against,
                            Points = apiTeam.Points,
                            LogoUrl = apiTeam.LogoUrl,
                            League = apiTeam.League
                        });

                        importedTeamNames.Add(apiTeam.Name.ToLower());
                    }
                }
            }

            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Teams updated from API-Football!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
