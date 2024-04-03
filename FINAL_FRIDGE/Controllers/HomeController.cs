using FINAL_FRIDGE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FINAL_FRIDGE.Data;

namespace FINAL_FRIDGE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        public IActionResult Index()
        {
            int _perPage = 5;

            var bookmarks = db.Bookmarks.Include("User").OrderByDescending(a => a.TotalLikes);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            int totalItems = bookmarks.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBookmarks = bookmarks.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);



            //ViewBag.bookmarks = bookmarks;

            var userId = _userManager.GetUserId(User);

            var userLikes = db.Likes
                .Where(l => l.UserId == userId)
                .Select(l => l.BookmarkId)
                .ToList();

            ViewData["UserLikes"] = userLikes;

            ViewBag.Bookmarks = paginatedBookmarks;

            return View();
        }

        public IActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks.Include("User")
                                         .Include("Comments")
                                         //.Include("Likes")
                                         .Include("Comments.User")
                                         .Where(book => book.Id == id)
                                         .First();

            ViewBag.UserCategories = db.Categories
                                      .Where(c => c.UserId == _userManager.GetUserId(User))
                                      .ToList();


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(bookmark);
        }


       
    }
}