using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    public class InjuriesController : Controller
    {
        private TeamContext db = new TeamContext();

        // GET: Injuries
        public ActionResult Index()
        {
            var injuries = db.Injuries.Include(i => i.Player).Include(i => i.Team).ToList();
            return View(injuries);
        }

        // GET: Injuries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Injury injury = db.Injuries.Find(id);
            if (injury == null)
            {
                return HttpNotFound();
            }
            return View(injury);
        }

        // GET: Injuries/Create
        public ActionResult Create()
        {
            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name");

            ViewBag.PlayerId = new SelectList(db.Players.Select(p => new
            {
                PlayerId = p.PlayerId,
                FullName = p.FirstName + " " + p.LastName
            }), "PlayerId", "FullName");

            return View();
        }


        // POST: Injuries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InjuryId,InjuryType,Severity,DateInjured,ExpectedRecoveryDate,Notes,TeamId,PlayerId")] Injury injury)
        {
            if (ModelState.IsValid)
            {
                db.Injuries.Add(injury);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", injury.TeamId);

            ViewBag.PlayerId = new SelectList(db.Players.Select(p => new
            {
                PlayerId = p.PlayerId,
                FullName = p.FirstName + " " + p.LastName
            }), "PlayerId", "FullName", injury.PlayerId);

            return View(injury);
        }



        // GET: Injuries/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Injury injury = db.Injuries.Find(id);
            if (injury == null)
            {
                return HttpNotFound();
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", injury.TeamId);

            ViewBag.PlayerId = new SelectList(db.Players.Select(p => new
            {
                PlayerId = p.PlayerId,
                FullName = p.FirstName + " " + p.LastName
            }), "PlayerId", "FullName", injury.PlayerId);

            return View(injury);
        }

        // POST: Injuries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InjuryId,InjuryType,Severity,DateInjured,ExpectedRecoveryDate,Notes,TeamId,PlayerId")] Injury injury)
        {
            if (ModelState.IsValid)
            {
                db.Entry(injury).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeamId = new SelectList(db.Teams, "TeamId", "Name", injury.TeamId);

            ViewBag.PlayerId = new SelectList(db.Players.Select(p => new
            {
                PlayerId = p.PlayerId,
                FullName = p.FirstName + " " + p.LastName
            }), "PlayerId", "FullName", injury.PlayerId);

            return View(injury);
        }

        // GET: Injuries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Injury injury = db.Injuries.Find(id);
            if (injury == null)
            {
                return HttpNotFound();
            }
            return View(injury);
        }

        // POST: Injuries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Injury injury = db.Injuries.Find(id);
            db.Injuries.Remove(injury);
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
