using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FootballManager.Models;

namespace FootballManager.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private TeamContext db = new TeamContext();

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Account user)
        {
            if (ModelState.IsValid)
            {
                var exists = db.Accounts.Any(u => u.Username == user.Username || u.Email == user.Email);
                if (exists)
                {
                    ViewBag.Message = "Username or Email already exists.";
                    return View(user);
                }

                db.Accounts.Add(user);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Login(string username, string password, bool rememberMe = false)
        {
            var user = db.Accounts.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;

                FormsAuthentication.SetAuthCookie(user.Username, rememberMe);

                if (rememberMe)
                {
                    Response.Cookies["username"].Value = username;
                    Response.Cookies["username"].Expires = DateTime.Now.AddDays(7);
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Invalid login attempt.";
            return View();
        }

        // /Account/Logout
        public ActionResult Logout()
        {
            if (Session["UserId"] != null)
            {
                Session.Abandon();
                FormsAuthentication.SignOut();
            }
            return RedirectToAction("Login");
        }

        public ActionResult LoggedIn()
        {
            if (Session["UserId"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
    }
}