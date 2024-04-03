using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FINAL_FRIDGE.Data;
using FINAL_FRIDGE.Models;
using System;



using Microsoft.AspNetCore.Http.Extensions;

namespace FINAL_FRIDGE.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        public BookmarksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;
        }

       
        //[Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            int _perPage = 5;

            var search = "";
   

            if(Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
            }


            var searchedBookmarks = db.Bookmarks.Include("User")
                                      .Where(book => book.Title.Contains(search)
                                        || book.Content.Contains(search))
                                      .OrderByDescending(b => b.TotalLikes)
                                      .ToList();

            

            var bookmarks = db.Bookmarks.Include("User").OrderByDescending(b => b.TotalLikes);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBookmarks = searchedBookmarks.Skip(offset).Take(_perPage);

            int totalItems = searchedBookmarks.Count();

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);


            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Bookmarks/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Bookmarks/Index/?page";
            }
 

        //ViewBag.bookmarks = bookmarks;

            var userId = _userManager.GetUserId(User);

            var userLikes = db.Likes
                .Where(l => l.UserId == userId)
                .Select(l => l.BookmarkId)
                .ToList();

            ViewData["UserLikes"] = userLikes;

            ViewBag.Bookmarks = paginatedBookmarks;

        
            ViewBag.Search = search;

            return View();
        }


        [Authorize(Roles = "User,Admin")]
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

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(bookmark);
        }


        // Adaugarea unui comentariu 
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + comment.BookmarkId);
            }

            else
            {
                Bookmark book = db.Bookmarks.Include("User")
                                         .Include("Comments")
                                         //.Include("Likes")
                                         .Include("Comments.User")
                                         .Where(book => book.Id == comment.BookmarkId)
                                         .First();


                // Adaugam bookmark-urile utilizatorului pentru dropdown
               ViewBag.Categories = db.Categories
                                         .Where(c => c.UserId == _userManager.GetUserId(User))
                                         .ToList();

                SetAccessRights();

                return View(book);
            }
        }

        [HttpPost]
        public IActionResult AddCategory([FromForm] BookmarkCategory bookmarkCategory)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {

                // Verificam daca avem deja bookmarkul in category
                if (db.BookmarkCategories
                    .Where(bc => bc.BookmarkId == bookmarkCategory.BookmarkId)
                    .Where(bc => bc.CategoryId == bookmarkCategory.CategoryId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest bookmark este deja adaugat in acest category";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                  
                    db.BookmarkCategories.Add(bookmarkCategory);
                 
                    db.SaveChanges();

                    // Adaugam un mesaj de succes
                    TempData["message"] = "Bookmarkul a fost adaugat in category-ul selectat";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga bookmarkul in category";
                TempData["messageType"] = "alert-danger";
            }


            return Redirect("/Bookmarks/Show/" + bookmarkCategory.BookmarkId);
        }

        [HttpPost]
        public IActionResult RemoveFromCategory(int bookmarkId, int categoryId)
        {
            // luam BC-ul 
            var bookmarkCategory = db.BookmarkCategories
                .FirstOrDefault(bc => bc.BookmarkId == bookmarkId && bc.CategoryId == categoryId);
            
            var categ = db.Categories
                           .Where(c => c.Id == categoryId)
                           .FirstOrDefault();

            if (categ.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (bookmarkCategory != null)
                {
                    try
                    {

                        db.BookmarkCategories.Remove(bookmarkCategory);
                        db.SaveChanges();

                        TempData["message"] = "Bookmarkul a fost sters din colectia selectata";
                        TempData["messageType"] = "alert-success";
                    }
                    catch (Exception ex)
                    {
                        TempData["message"] = "Nu s-a putut sterge bookmarkul din colectie. Eroare: " + ex.Message;
                        TempData["messageType"] = "alert-danger";
                    }
                }
                else
                {
                    TempData["message"] = "Bookmarkul nu a fost gasit in colectia selectata";
                    TempData["messageType"] = "alert-danger";
                }
            }

            TempData["message"] = "Bookmarkul a fost sters din colectia selectata";
            TempData["messageType"] = "alert-success";
            return Redirect("/Categories/Show/" + categ.Id);
        }

        // Se afiseaza formularul in care se vor completa datele unui bookmark

        [Authorize(Roles = "User,Admin")]
        public IActionResult New(/*bool? url_link*/)
        {
            //ViewBag.Url_Link = true;

     

            //if (url_link == false) ViewBag.Url_Link = false;

            Bookmark bookmark = new Bookmark();

            return View(bookmark);
        }

       

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Bookmark bookmark, IFormFile? BookmarkImage, string? EBD)
        {
            bookmark.Date = DateTime.Now;


            bookmark.UserId = _userManager.GetUserId(User);

            if (BookmarkImage != null && BookmarkImage.Length > 0 && BookmarkImage is IFormFile)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "images", BookmarkImage.FileName);
                var databaseFileName = "/images/" + BookmarkImage.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    BookmarkImage.CopyTo(fileStream);
                }

                bookmark.Image = databaseFileName;
            }
            else if(EBD != null && EBD.Length > 0)
            {
               
                bookmark.Image = EBD;
            }

            if (ModelState.IsValid)
            {
                db.Bookmarks.Add(bookmark);
                db.SaveChanges();
                TempData["message"] = "Bookmark-ul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(bookmark);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {

            Bookmark bookmark = db.Bookmarks.Where(book => book.Id == id)
                                        .First();

            var usr = db.ApplicationUsers.Find(_userManager.GetUserId(User));



            if (bookmark.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(bookmark);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bookmark care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }



        // Verificam rolul utilizatorilor care au dreptul sa editeze
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Bookmark requestBookmark, IFormFile? UpdatedImage, string? EBD)
        {
            
            Bookmark bookmark = db.Bookmarks.Find(id);

            requestBookmark.Image = bookmark.Image;

            if (ModelState.IsValid)
            {
                // permisia
                if (bookmark.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    
                    bookmark.Title = requestBookmark.Title;
                    bookmark.Content = requestBookmark.Content;

                    if (UpdatedImage != null && UpdatedImage.Length > 0)
                    {
                        
                        var storagePath = Path.Combine(_env.WebRootPath, "images", UpdatedImage.FileName);
                        var databaseFileName = "/images/" + UpdatedImage.FileName;


                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            UpdatedImage.CopyTo(fileStream);
                        }

                        bookmark.Image = databaseFileName;
                    }
                    else if (EBD != null && EBD.Length > 0)
                    {

                        bookmark.Image = EBD;
                    }



                    db.SaveChanges();

                    TempData["message"] = "Bookmark-ul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bookmark care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                
                return View(requestBookmark);
            }
        }




        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Bookmark bookmark = db.Bookmarks.Include("Comments")
                                         .Include("Likes")
                                         .Where(book => book.Id == id)
                                         .First();

            if (bookmark.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Bookmarks.Remove(bookmark);
                db.Likes.RemoveRange(bookmark.Likes);
                db.SaveChanges();
                TempData["message"] = "Bookmark-ul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un bookmark care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
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

        
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            string userId = _userManager.GetUserId(User);

            var categories = db.Categories
                            .Where(c => c.UserId == userId);
                           // .ToList();

            
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name.ToString()
                });
            }

            return selectList;
        }



        public List<int?> GetCurrentUserLikes()
        {
            List<int?> userLikes = new List<int?>();

            if (User.Identity.IsAuthenticated)
            {
                string userId = _userManager.GetUserId(User);

                var likedBookmarks = db.Likes
                                       .Where(l => l.UserId == userId)
                                       .Select(l => l.BookmarkId)
                                       .ToList(); 

                userLikes.AddRange(likedBookmarks);
            }

            return userLikes;
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Like(int bookmarkId)
        {

            string userId = _userManager.GetUserId(User);


            if (!db.Likes.Any(l => l.BookmarkId == bookmarkId && l.UserId == userId))
            {
                // Create a new like
                var like = new Like
                {
                    BookmarkId = bookmarkId,
                    UserId = userId
                };
                var book = db.Bookmarks.Find(bookmarkId);

                book.TotalLikes++;

                db.Likes.Add(like);
                db.SaveChanges();

                ViewData["UserLikes"] = GetCurrentUserLikes();
            }

            return RedirectToAction("Index", new { id = bookmarkId });

        }

        [HttpPost]
        public IActionResult Unlike(int bookmarkId)
        {
  
            var userId = _userManager.GetUserId(User);

            var like = db.Likes.FirstOrDefault(l => l.BookmarkId == bookmarkId && l.UserId == userId);

            if (like != null)
            {
                var book = db.Bookmarks.Find(bookmarkId);

                book.TotalLikes--;

                db.Likes.Remove(like);
                db.SaveChanges();

                ViewData["UserLikes"] = GetCurrentUserLikes();
            }


            return RedirectToAction("Index", new { id = bookmarkId });
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllLikes()
        {

            var selectList = new List<SelectListItem>();
 
            var likes = from lk in db.Likes
                        //where lk.BookmarkId = bookmarkId
                             select lk;
 
            foreach (var like in likes)
            {
                
                selectList.Add(new SelectListItem
                {
                    Value = like.Id.ToString(),
                    Text = like.BookmarkId.ToString()
                });
            }
            return selectList;
        }
        

        public IActionResult IndexNou()
        {
            return View();
        }
    }
}

