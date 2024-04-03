using FINAL_FRIDGE.Data;
using FINAL_FRIDGE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FINAL_FRIDGE.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CategoriesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        // toti utilizatorii pot vedea Bookmark-urile existente in platforma
        // fiecare utilizator vede bookmark-urile pe care le-a creat

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index(string userId)
        {

            
            if (userId == null)
            {
                userId = _userManager.GetUserId(User);
            }

            ViewBag.CurrentUserId = userId;

            var UserName = (from usr in db.Users
                            where usr.Id == userId
                            select usr.UserName).FirstOrDefault();

            ViewBag.UserName = UserName;

            var userPP = (from usr in db.Users
                        where usr.Id == userId
                        select usr.ProfilePicture).FirstOrDefault();

            ViewBag.UserProfilePicture = userPP;


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User"))
            {
                var categories = from categ in db.Categories.Include("User")
                               .Where(b => b.UserId == userId)
                                select categ;

                ViewBag.Categories = categories;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var categories = from categ in db.Categories.Include("User")
                                 .Where(b => b.UserId == userId)
                                select categ;

                ViewBag.Categories = categories;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra categoriei";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Bookmarks");
            }

        }

        // Afisarea tuturor bookmarkurilor pe care utilizatorul le-a salvat
        // in categorii

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            int _perPage = 5;

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            ViewBag.CurrentPage = currentPage;

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var BookCat = db.BookmarkCategories.Include(bc => bc.Bookmark)
                .Where(bc => bc.CategoryId == id).ToList();
  
            var bookmarks = BookCat.Select(bc => bc.Bookmark);

            var paginatedBookmarks = bookmarks.Skip(offset).Take(_perPage);

            int totalItems = bookmarks.Count();

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            var categories = db.Categories
                                  .Include("BookmarkCategories.Bookmark.User")
                                  .Include("User")
                                  .Where(c => c.Id == id)
                                  .FirstOrDefault();


                if (categories == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Bookmarks");
                }


                return View(categories);
            
            
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult New(Category cat)
        {
            cat.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Categoia a fost adaugata";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                return View(cat);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Category categ = db.Categories.Where(cat => cat.Id == id)
                                        .First();

            if (categ.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(categ);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei categori care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Category requestCategory)
        {

            Category categ = db.Categories.Find(id);

            try 
            {
                categ.Name = requestCategory.Name;

                categ.Note = requestCategory.Note;

                categ.BookmarkCategories = categ.BookmarkCategories;

                db.SaveChanges();

                return RedirectToAction("Show", new { categ.Id });
            }
            catch (Exception)
            { 
                return RedirectToAction("Edit", categ.Id);
            }
            
        }


            public ActionResult Delete(int id)
        {
         

            var bookmarkCategories = db.BookmarkCategories
                                    .Where(cat => cat.Id == id);

            var categ = db.Categories.Include(c => c.BookmarkCategories)
                                      .ThenInclude(bc => bc.Bookmark)
                                      .FirstOrDefault(c => c.Id == id);

            if (categ.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (categ != null)
                {
                    foreach (var bookmarkCategory in categ.BookmarkCategories)
                    {
                        db.BookmarkCategories.Remove(bookmarkCategory);
                    }

                    db.Categories.Remove(categ);

                    db.SaveChanges();
                    TempData["message"] = "Categoria a fost stersa";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index", new { categ.UserId });
                }
                return RedirectToAction("Index", new { categ.UserId });
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o categorie care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", new { categ.UserId });
            }
        }


        // Conditiile de afisare a butoanelor 
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}
