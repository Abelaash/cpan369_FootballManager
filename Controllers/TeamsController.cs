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
            return View(db.Teams.ToList());
        }

        // GET: Teams/Details/5
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
                .Where(m =>
                    m.Status == "Upcoming" &&
                    m.MatchDate > DateTime.UtcNow &&
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
