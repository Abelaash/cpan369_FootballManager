using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    public class PlayersController : Controller
    {
        private TeamContext db = new TeamContext();

        [Authorize]
        public ActionResult Index(string leagueName)
        {
            var players = db.Players.Include(p => p.Team);

            if (!string.IsNullOrEmpty(leagueName))
            {
                players = players.Where(p => p.Team.League == leagueName);
            }

            // Predefined list of league names for dropdown
            var leagues = new List<string>
            {
                "Premier League",
                "La Liga",
                "Bundesliga",
                "Ligue 1",
                "Serie A",
                "Eredivisie"
            };

            ViewBag.LeagueName = new SelectList(leagues, leagueName); // second arg sets selected value

            return View(players.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Player player = db.Players.Find(id);
            db.Players.Remove(player);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ImportMultipleTeamsFromApi()
        {
            var apiTeamIds = new List<int> { 157, 168, 169, 173, 164, 160, 163, 165, 170, 172, 162, 161, 182, 167, 186, 180, 176, 191 }; // replace with your actual team IDs
            int season = 2024;

            foreach (var teamId in apiTeamIds)
            {
                await ImportSingleTeamFromApi(teamId, season);
                await Task.Delay(4000); // wait 2 seconds between calls to respect rate limit
            }

            TempData["SuccessMessage"] = "All teams imported successfully!";
            return RedirectToAction("Index");
        }

        private async Task ImportSingleTeamFromApi(int apiTeamId, int season)
        {
            var service = new ApiFootballService();

            var apiPlayers = await service.GetPlayersByTeamAsync(apiTeamId, season);
            string apiTeamName = apiPlayers.FirstOrDefault()?.TeamName?.ToLower();

            if (string.IsNullOrWhiteSpace(apiTeamName))
                return;

            var localTeam = db.Teams.FirstOrDefault(t => t.Name.ToLower() == apiTeamName);
            if (localTeam == null)
                return;

            int localTeamId = localTeam.TeamId;

            var existingPlayerNames = new HashSet<string>(
                db.Players.Where(p => p.TeamId == localTeamId)
                          .Select(p => (p.FirstName + " " + p.LastName).ToLower())
            );

            foreach (var player in apiPlayers)
            {
                string fullName = (player.FirstName + " " + player.LastName).ToLower();
                if (existingPlayerNames.Contains(fullName)) continue;

                if (string.IsNullOrWhiteSpace(player.Position))
                    player.Position = "Unknown";

                if (player.JerseyNumber < 1 || player.JerseyNumber > 99)
                    player.JerseyNumber = 0;

                player.TeamId = localTeamId;
                db.Players.Add(player);
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



        public async Task<ActionResult> ImportFromApi(int apiTeamId, int season = 2024)
        {
            var service = new ApiFootballService();

            // Get players from the API using the API Team ID
            var apiPlayers = await service.GetPlayersByTeamAsync(apiTeamId, season);

            string apiTeamName = apiPlayers.FirstOrDefault()?.TeamName?.ToLower();

            if (string.IsNullOrWhiteSpace(apiTeamName))
            {
                TempData["ErrorMessage"] = "Team name not found in API response.";
                return RedirectToAction("Index");
            }

            var localTeam = db.Teams.FirstOrDefault(t => t.Name.ToLower() == apiTeamName);

            if (localTeam == null)
            {
                TempData["ErrorMessage"] = $"No matching team found in local DB for '{apiTeamName}'.";
                return RedirectToAction("Index");
            }

            int localTeamId = localTeam.TeamId;

            // Existing players in the local DB for this team
            var existingPlayerNames = new HashSet<string>(
                db.Players.Where(p => p.TeamId == localTeamId)
                          .Select(p => (p.FirstName + " " + p.LastName).ToLower())
            );

            foreach (var player in apiPlayers)
            {
                string fullName = (player.FirstName + " " + player.LastName).ToLower();

                if (existingPlayerNames.Contains(fullName))
                    continue;

                if (string.IsNullOrWhiteSpace(player.Position))
                    player.Position = "Unknown";

                if (player.JerseyNumber < 1 || player.JerseyNumber > 99)
                    player.JerseyNumber = 0;

                player.TeamId = localTeamId;

                db.Players.Add(player);
            }

            try
            {
                db.SaveChanges();
                TempData["SuccessMessage"] = "Players imported successfully!";
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

                TempData["ErrorMessage"] = "Player import failed due to validation errors.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }



        //public async Task<ActionResult> ImportFromApi(int teamId, int season = 2024)
        //{
        //    var service = new ApiFootballService();
        //    var apiPlayers = await service.GetPlayersByTeamAsync(teamId, season);

        //    var existingPlayerNames = new HashSet<string>(
        //        db.Players.Where(p => p.TeamId == teamId)
        //                  .Select(p => (p.FirstName + " " + p.LastName).ToLower())
        //    );

        //    foreach (var wrapper in apiPlayers)
        //    {
        //        var apiPlayer = wrapper.player;
        //        if (apiPlayer == null || string.IsNullOrWhiteSpace(apiPlayer.name)) continue;

        //        if (existingPlayerNames.Contains(apiPlayer.name.ToLower())) continue;

        //        var names = apiPlayer.name.Split(' ');
        //        string firstName = names.FirstOrDefault() ?? "Unknown";
        //        string lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "Unknown";

        //        int age = (apiPlayer.age >= 16 && apiPlayer.age <= 45) ? apiPlayer.age : 25;
        //        int jerseyNumber = apiPlayer.number ?? 0;
        //        string position = string.IsNullOrWhiteSpace(apiPlayer.position) ? "Unknown" : apiPlayer.position;

        //        decimal height = ParseHeight(apiPlayer.statistics?.FirstOrDefault()?.height) ?? 1.75M;
        //        decimal weight = ParseWeight(apiPlayer.statistics?.FirstOrDefault()?.weight) ?? 70M;

        //        db.Players.Add(new Player
        //        {
        //            FirstName = firstName,
        //            LastName = lastName,
        //            Position = position,
        //            JerseyNumber = jerseyNumber,
        //            Age = age,
        //            Height = height,
        //            Weight = weight,
        //            TeamId = teamId
        //        });
        //    }

        //    try
        //    {
        //        db.SaveChanges();
        //        TempData["SuccessMessage"] = "Players imported successfully!";
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
        //    {
        //        foreach (var validationErrors in ex.EntityValidationErrors)
        //        {
        //            foreach (var error in validationErrors.ValidationErrors)
        //            {
        //                System.Diagnostics.Debug.WriteLine($"Validation Error - Property: {error.PropertyName}, Error: {error.ErrorMessage}");
        //            }
        //        }
        //        TempData["ErrorMessage"] = "Player import failed due to validation errors.";
        //        return RedirectToAction("Index");
        //    }

        //    return RedirectToAction("Index");
        //}

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
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
