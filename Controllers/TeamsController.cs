using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    public class TeamsController : Controller
    {
        private TeamContext db = new TeamContext();

        // GET: Teams
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.LeagueList = GetLeagueList(); 
            var teams = db.Teams.ToList();
            return View(teams);
        }

        public ActionResult GetTeamsByLeague(string leagueName)
        {
            var teams = db.Teams
                          .Where(t => t.League == leagueName)
                          .ToList();

            return PartialView("_TeamTablePartial", teams);
        }

        private List<LeagueOption> GetLeagueList()
        {
            return new List<LeagueOption>
            {
                new LeagueOption { Name = "Premier League", LogoUrl = "https://media-4.api-sports.io/football/leagues/39.png" },
                new LeagueOption { Name = "La Liga", LogoUrl = "https://media-4.api-sports.io/football/leagues/140.png" },
                new LeagueOption { Name = "Bundesliga", LogoUrl = "https://media-4.api-sports.io/football/leagues/78.png" },
                new LeagueOption { Name = "Serie A", LogoUrl = "https://media-4.api-sports.io/football/leagues/135.png" },
                new LeagueOption { Name = "Ligue 1", LogoUrl = "https://media-4.api-sports.io/football/leagues/61.png" },
                new LeagueOption { Name = "Eredivisie", LogoUrl = "https://media-4.api-sports.io/football/leagues/88.png" },
            };
        }

        // GET: Teams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var team = db.Teams.Find(id);
            if (team == null)
                return HttpNotFound();

            var apiService = new ApiFootballService();
            var apiTeamId = apiService.GetApiTeamId(team.Name);
            var fixtures = apiService.GetUpcomingMatchesByTeam(apiTeamId);

            var squad = db.Players.Where(p => p.TeamId == team.TeamId).ToList();

            var viewModel = new TeamDetailsViewModel
            {
                Team = team,
                UpcomingMatches = fixtures,
                Squad = squad
            };

            return View(viewModel);
        }

        // GET: Teams/Create
        public ActionResult Create() => View();

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

        // GET: Teams/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var team = db.Teams.Find(id);
            return team == null ? HttpNotFound() : (ActionResult)View(team);
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
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var team = db.Teams.Find(id);
            return team == null ? HttpNotFound() : (ActionResult)View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var team = db.Teams.Find(id);
            db.Teams.Remove(team);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Import Fixtures from API
        public async Task<ActionResult> ImportFixturesFromApi()
        {
            var service = new ApiFootballService();
            var leagues = new List<string> { "Premier League", "La Liga", "Serie A", "Bundesliga", "Ligue 1", "Eredivisie" };

            foreach (var league in leagues)
            {
                var fixtures = service.GetUpcomingMatches(league);

                foreach (var fixture in fixtures)
                {
                    // Skip past or invalid match
                    if (fixture.MatchDate <= DateTime.UtcNow) continue;

                    var homeTeam = db.Teams.FirstOrDefault(t => t.Name == fixture.HomeTeamName);
                    var awayTeam = db.Teams.FirstOrDefault(t => t.Name == fixture.AwayTeamName);

                    if (homeTeam == null || awayTeam == null) continue;

                    bool alreadyExists = db.Matches.Any(m =>
                        m.HomeTeamId == homeTeam.TeamId &&
                        m.AwayTeamId == awayTeam.TeamId &&
                        m.MatchDate == fixture.MatchDate);

                    if (!alreadyExists)
                    {
                        fixture.HomeTeamId = homeTeam.TeamId;
                        fixture.AwayTeamId = awayTeam.TeamId;
                        fixture.Status = "Upcoming";

                        db.Matches.Add(fixture);
                        System.Diagnostics.Debug.WriteLine($"✔ Imported: {homeTeam.Name} vs {awayTeam.Name} on {fixture.MatchDate}");
                    }
                }
            }

            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Fixtures imported successfully!";
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
