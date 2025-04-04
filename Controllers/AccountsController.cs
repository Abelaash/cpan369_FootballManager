using FootballManager.Models;
using System.Linq;
using System.Web.Mvc;

namespace FootballManager.Controllers
{
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
                // Check if username or email already exists
                var exists = db.Accounts.Any(u => u.Username == user.Username || u.Email == user.Email);
                if (exists)
                {
                    ViewBag.Message = "Username or Email already exists.";
                    return View(user);
                }

                db.Accounts.Add(user);
                db.SaveChanges();
                ViewBag.Message = "Registration successful!";
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
        public ActionResult Login(string username, string password)
        {
            var user = db.Accounts.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                Session["UserId"] = user.userId;
                Session["Username"] = user.Username;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Invalid login attempt.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
