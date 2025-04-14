using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    public class StaffsController : AsyncController
    {
        private TeamContext db = new TeamContext();

        // GET: Staffs
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.LeagueList = GetLeagueList();

            var staffList = db.Staff
                              .Include(s => s.Team)
                              .ToList();

            return View(staffList);
        }

        public ActionResult GetStaffByLeague(string leagueName)
        {
            var staff = db.Staff
                  .Include(s => s.Team)
                  .Where(s => s.Team.League == leagueName)
                  .ToList();

            return PartialView("_StaffTablePartial", staff);
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

        // GET: Staffs/Create
        public ActionResult Create()
        {
            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name");
            return View();
        }

        // POST: Staffs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StaffId,FirstName,LastName,Role,TeamId")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                db.Staff.Add(staff);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", staff.TeamId);

            return View(staff);
        }

        // GET: Staffs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = db.Staff.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", staff.TeamId);
            return View(staff);
        }

        // POST: Staffs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "StaffId,FirstName,LastName,Role,TeamId")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", staff.TeamId);
            return View(staff);
        }

        // GET: Staffs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = db.Staff.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Staff staff = db.Staff.Find(id);
            db.Staff.Remove(staff);
            db.SaveChanges();
            return RedirectToAction("Index");
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