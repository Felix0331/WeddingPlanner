using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("create")]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == user.Email))
                {

                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");

                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                dbContext.Add(user);
                dbContext.SaveChanges();

                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                HttpContext.Session.SetInt32("UserID", userInDb.UserId);

                return RedirectToAction("Success");
            }
            else
            {

                return View("Index");
            }
        }
        [HttpPost("loginuser")]

        public IActionResult LoginUser(LoginChecker user)
        {
            System.Console.WriteLine($"@@@@@@@@@@@@@@@@@@@@@@@@@");

            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.LoginEmail);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserID", userInDb.UserId);

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginChecker>();
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.LoginPassword);
                // result can be compared to 0 for failure
                if (result == 0)
                {
                    ModelState.AddModelError("Password", "Check your Password!");
                    // You may consider returning to the View at this point
                    return View("Index");
                }
                return RedirectToAction("Success");
            }
            return View("Index");
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet("Dashboard")]
        public IActionResult Success()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Index");
            }
            int? UserID = HttpContext.Session.GetInt32("UserID");
            // ViewBag.GuestId = UserID;
            var userInDb = dbContext.Users.FirstOrDefault(u => u.UserId == UserID);
            ViewBag.CreatorId = (int)UserID;
            List<Wedding> AllWeddings = dbContext.Weddings
            .Include(k => k.Guests)
            .ToList();
            ViewBag.RSVPBool = dbContext.RSVPs.Any(u => u.UserId == UserID);
            ViewBag.RSVPBoolW = dbContext.RSVPs.Any(u => u.WeddingId == UserID);
            ViewBag.ListOfRSVPs = dbContext.RSVPs.ToList();
            return View(AllWeddings);
        }

        [HttpGet("MakeWedding")]
        public IActionResult RenderWeddingForm()
        {
            int? CurrUser = HttpContext.Session.GetInt32("UserID");
            ViewBag.Planner = CurrUser;

            return View("WeddingForm");
        }
        // BReaks here
        [HttpPost("CreateWedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                if (HttpContext.Session.GetInt32("UserID") == null)
                {
                    ModelState.AddModelError("Password", "Session has ended, please login again.");
                    return RedirectToAction("Index");
                }
                dbContext.Add(newWedding);
                dbContext.SaveChanges();
                return Redirect("weddingdetails/" + newWedding.WeddingId);

            }
            else
            {
                return RedirectToAction("WeddingForm");
            }
        }

        [HttpGet("weddingdetails/{WedId}")]
        public IActionResult WeddingDetails(int WedId)
        {
            Console.WriteLine("*************************************************************************");

            var wedding = dbContext.Weddings.FirstOrDefault(p => p.WeddingId == WedId);
            var wedguests = dbContext.Weddings
            .Include(p => p.Guests)
            .ThenInclude(p => p.User)
            .FirstOrDefault(p => p.WeddingId == WedId);

            ViewBag.Wedder1 = wedding.WedderOne;
            ViewBag.Wedder2 = wedding.WedderTwo;
            ViewBag.WeddingDate = wedding.WeddingDate;
            if (wedding.Guests == null)
            {
                ViewBag.GuestList = "NoGuest";
            }
            else {
            ViewBag.GuestList  = wedding.Guests.ToList();
            }
            Console.WriteLine(wedguests);
            Console.WriteLine("*************************************************************************");

            return View();
        }

        [HttpGet("rsvp/{WedId}")]
        public IActionResult RSVP(int WedId)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                //Need logic to prevent double rsvp
                int? GuestId = HttpContext.Session.GetInt32("UserID");
                RSVP RsVar = new RSVP();
                RsVar.WeddingId = WedId;
                RsVar.UserId = (int)GuestId;
                dbContext.Add(RsVar);
                dbContext.SaveChanges();
                return RedirectToAction("Success");
            }
        }
        [HttpGet("unrsvp/{WedId}")]
        public IActionResult UNRSVP(int WedId)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                int? GuestId = HttpContext.Session.GetInt32("UserID");
                GuestId = (int)GuestId;
                RSVP PersonToWedding = dbContext.RSVPs.FirstOrDefault(p => p.WeddingId == WedId && p.UserId == GuestId);

                dbContext.Remove(PersonToWedding);
                dbContext.SaveChanges();
                return RedirectToAction("Success");
            }
        }

        [HttpGet("deletewedding/{WedId}")]
        public IActionResult DelteWedding(int WedId)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                int? GuestId = HttpContext.Session.GetInt32("UserID");
                GuestId = (int)GuestId;
                Wedding WeddingToDelete = dbContext.Weddings.FirstOrDefault(p => p.WeddingId == WedId);

                dbContext.Remove(WeddingToDelete);
                dbContext.SaveChanges();
                // =====================
                List<RSVP> AllWeddingRSVPs = dbContext.RSVPs.Where(p => p.WeddingId == WedId).ToList();
                foreach (var rsvp in AllWeddingRSVPs)
                {
                    dbContext.Remove(rsvp);
                    dbContext.SaveChanges();
                }
                return RedirectToAction("Success");
            }
        }
    }
}
