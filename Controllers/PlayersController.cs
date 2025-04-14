using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    public class PlayersController : Controller
    {
        private TeamContext db = new TeamContext();

        [Authorize]
        public ActionResult Index()
        {
            string defaultLeague = "Premier League";
            string defaultTeam = "Manchester United";

            var players = db.Players
                .Include(p => p.Team)
                .Where(p => p.Team.Name == defaultTeam)
                .ToList();

            ViewBag.LeagueList = GetLeagueList(); // Same as your existing list
            ViewBag.SelectedLeague = defaultLeague;
            ViewBag.TeamList = GetTeamsByLeague(defaultLeague);
            ViewBag.SelectedTeam = defaultTeam;

            return View(players);
        }

        [HttpGet]
        public JsonResult GetTeams(string leagueName)
        {
            var teams = GetTeamsByLeague(leagueName);
            return Json(teams, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public PartialViewResult GetPlayers(string teamName)
        {
            var players = db.Players.Include(p => p.Team)
                .Where(p => p.Team.Name == teamName)
                .ToList();

            return PartialView("_PlayerTablePartial", players);
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

        private List<string> GetTeamsByLeague(string leagueName)
        {
            return db.Teams
                .Where(t => t.League == leagueName)
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToList();
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var player = db.Players.Find(id);
            if (player == null)
                return HttpNotFound();

            return View(player);
        }

        public ActionResult Create()
        {
            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PlayerId,FirstName,LastName,Position,JerseyNumber,Age,Height,Weight,TeamId")] Player player)
        {
            if (ModelState.IsValid)
            {
                db.Players.Add(player);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var player = db.Players.Find(id);
            if (player == null)
                return HttpNotFound();

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PlayerId,FirstName,LastName,Position,JerseyNumber,Age,Height,Weight,TeamId")] Player player)
        {
            if (ModelState.IsValid)
            {
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", player.TeamId);
            return View(player);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var player = db.Players.Find(id);
            if (player == null)
                return HttpNotFound();

            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var player = db.Players.Find(id);
            db.Players.Remove(player);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ImportMultipleTeamsFromApi()
        {
            var apiTeamIds = new List<int> { 33, 34, 35, 36, 39, 40, 41, 42, 45, 46, 47, 48, 49, 50, 51, 52, 55, 57, 65, 66 };
            int season = 2024;

            foreach (var teamId in apiTeamIds)
            {
                await ImportSingleTeamFromApi(teamId, season);
                await Task.Delay(4000); // avoid rate-limiting
            }

            TempData["SuccessMessage"] = "All teams imported successfully!";
            return RedirectToAction("Index");
        }

        private async Task ImportSingleTeamFromApi(int apiTeamId, int season)
        {
            var service = new ApiFootballService();
            var apiPlayers = await service.GetPlayersByTeamAsync(apiTeamId, season);

            string apiTeamName = apiPlayers.FirstOrDefault()?.TeamName?.ToLower();
            if (string.IsNullOrWhiteSpace(apiTeamName)) return;

            var localTeam = db.Teams.FirstOrDefault(t => t.Name.ToLower() == apiTeamName);
            if (localTeam == null) return;

            int localTeamId = localTeam.TeamId;

            foreach (var player in apiPlayers)
            {
                string fullName = (player.FirstName + " " + player.LastName).ToLower();

                var existingPlayer = db.Players.FirstOrDefault(p =>
                    (p.FirstName + " " + p.LastName).ToLower() == fullName &&
                    p.TeamId == localTeamId);

                if (existingPlayer != null)
                {
                    existingPlayer.Appearances = player.Appearances;
                    existingPlayer.Assists = player.Assists;
                    existingPlayer.TotalGoals = player.TotalGoals;
                    existingPlayer.TotalShots = player.TotalShots;
                    existingPlayer.TotalPasses = player.TotalPasses;
                    existingPlayer.PhotoUrl = player.PhotoUrl;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(player.Position))
                        player.Position = "Unknown";

                    if (player.JerseyNumber < 1 || player.JerseyNumber > 99)
                        player.JerseyNumber = 0;

                    player.TeamId = localTeamId;
                    db.Players.Add(player);
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var error in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error - Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                    }
                }
            }
        }

        private decimal? ParseHeight(string height)
        {
            if (decimal.TryParse(height?.Replace("cm", "").Trim(), out decimal value))
                return value / 100;
            return null;
        }

        private decimal? ParseWeight(string weight)
        {
            if (decimal.TryParse(weight?.Replace("kg", "").Trim(), out decimal value))
                return value;
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
