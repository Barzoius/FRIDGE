using FINAL_FRIDGE.Data;
using FINAL_FRIDGE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FINAL_FRIDGE.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
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
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;


            return View();
        }

        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;

            return View(user);
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); // Lista de nume de roluri

            // Cautam ID-ul rolului in baza de date
            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .First(); // Selectam 1 singur rol
            ViewBag.UserRole = currentUserRole;

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();


            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;


                // Cautam toate rolurile din baza de date
                var roles = db.Roles.ToList();

                foreach (var role in roles)
                {
                    // Scoatem userul din rolurile anterioare
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                // Adaugam noul rol selectat
                var roleName = await _roleManager.FindByIdAsync(newRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                db.SaveChanges();

            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = db.Users
                         .Include("Comments")
                         .Include("Bookmarks")
                         .Where(u => u.Id == id)
                         .First();

            // Delete user comments
            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // Delete user bookmarks
            if (user.Bookmarks.Count > 0)
            {
                foreach (var bookmark in user.Bookmarks)
                {

                    db.Bookmarks.Remove(bookmark);
                    //db.Likes.RemoveRange(bookmark.Likes);
                }
            }

            var userLikes = db.Likes.Where(l => l.UserId == id);

            foreach (var lk in userLikes)
            {
                
                var likedBookmark = db.Bookmarks.SingleOrDefault(b => b.Id == lk.BookmarkId);

                if (likedBookmark != null)
                {
                    likedBookmark.TotalLikes--; 
                    db.Likes.Remove(lk);
                }

            }

            var categ = db.Categories.Include(c => c.BookmarkCategories)
                                         .ThenInclude(bc => bc.Bookmark)
                                         .FirstOrDefault(cat => cat.UserId == id);

            if (categ != null)
            {
                foreach (var bookmarkCategory in categ.BookmarkCategories)
                {
                    db.BookmarkCategories.Remove(bookmarkCategory);
                }

                db.Categories.Remove(categ);

                //db.SaveChanges();
            }



            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }


        public IActionResult UserProfile(string userId)
        {

            int _perPage = 5;

            var bookmarks = db.Bookmarks.Include("User")
                            .Where(b => b.UserId == userId )
                            .OrderByDescending(a => a.TotalLikes);

           
            int totalItems = bookmarks.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            ViewBag.CurrentPage = currentPage;

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBookmarks = bookmarks.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // userii si bookmarkurile
          
            if (userId == null)
            {
                userId = _userManager.GetUserId(User);
            }

            var user = db.Users.Include(u => u.Bookmarks).SingleOrDefault(u => u.Id == userId);
            
            //var user = db.Users.Include("Bookmarks")
            // .Where(u => u.Id == userId)
            // .FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {

                return NotFound();
            }

            var categories = from categ in db.Categories.Include("User")
                               .Where(b => b.UserId == userId)
                             select categ;

            ViewBag.Categories = categories;
            
            ViewBag.UserCurent = _userManager.GetUserId(User);

            //bookmarkurile placute de user
            var likedBookmarks = db.Likes
               .Where(l => l.UserId == userId)
                .Select(l => l.Bookmark)
                .ToList();
            
            ViewBag.Bookmarks = paginatedBookmarks;

            return View(user);
        }
    }
}
