using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using FootballManager.API;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    public class StaffsController : Controller
    {
        private TeamContext db = new TeamContext();

        // GET: Staffs
        [Authorize]
        [Authorize]
        public ActionResult Index()
        {
            var service = new ApiFootballService();
            var leagues = new List<string> { "Premier League" }; // , "La Liga", "Bundesliga", "Ligue 1", "Serie A", "Eredivisie"

            foreach (var league in leagues)
            {
                var coaches = service.GetCoachesByLeague(league); // This was missing!

                foreach (var coach in coaches)
                {
                    var team = db.Teams.FirstOrDefault(t => t.Name.Trim().ToLower() == coach.Team.Trim().ToLower());

                    if (team != null && !db.Staff.Any(s =>
                        s.FirstName.Trim().ToLower() == coach.FirstName.Trim().ToLower() &&
                        s.LastName.Trim().ToLower() == coach.LastName.Trim().ToLower() &&
                        s.Role == "Coach" &&
                        s.TeamId == team.TeamId))
                    {
                        db.Staff.Add(new Staff
                        {
                            FirstName = coach.FirstName,
                            LastName = coach.LastName,
                            Role = "Coach",
                            TeamId = team.TeamId
                        });
                    }
                }
            }


            db.SaveChanges(); // ✅ Moved inside method scope

            var staffList = db.Staff.Include(s => s.Team).ToList();
            return View(staffList);
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
