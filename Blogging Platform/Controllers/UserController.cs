using Blogging_Platform.Models;
using Blogging_Platform.Repositories;
using Blogging_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Security.Claims;

namespace Blogging_Platform.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly MyDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IUserRepository userRepository;

        public UserController(UserManager<AppUser> userManager, IUserRepository userRepository, MyDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.userRepository = userRepository;
        }

        // GET: UserController
        // show analysis of the user account

        [Authorize(Roles = "user")]
        public async Task<ActionResult> Dashboard()
        {
            var CurrentUser = await userManager.GetUserAsync(User);

            ViewBag.ArticlesNumber = dbContext.Articles.Where(a => a.UserId == CurrentUser.Id).ToList().Count;
            ViewBag.FollowersNumber = dbContext.Follows.Where(f => f.FolloweeId == CurrentUser.Id).ToList().Count;
            ViewBag.ActionsNumber = dbContext.Actions.Where(a => a.UserId == CurrentUser.Id).ToList().Count;

            /*Calculate articles views*/

            var userArticles = dbContext.Articles.Where(a => a.UserId == CurrentUser.Id).ToList();
            int Counter = 0;
            foreach (var x in userArticles) {
                if (x.ViewCount != 0) {
                    Counter += x.ViewCount;
                }
            }
            ViewBag.ArticlesViewsNumber = Counter;

            ViewBag.UserArticles = (from a in dbContext.Articles where a.UserId == CurrentUser.Id select a).ToList();
            return View();
        }

        // GET: UserController/Settings/
        // configure profile settings

        [Authorize(Roles = "user")]
        public async Task<ActionResult> Settings()
        {
            ViewBag.Country = new SelectList(dbContext.Countries, "Id", "Name");

            var user = await userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<ActionResult> Settings(AppUser currentUser, IFormFile ProfilePicture)
        {
            try
            {
                if (ProfilePicture != null)
                {
                    var guid = Guid.NewGuid();
                    var wwroot = webHostEnvironment.WebRootPath + "/ProfilePictures";
                    var fullPath = System.IO.Path.Combine(wwroot, guid + ProfilePicture.FileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        ProfilePicture.CopyTo(stream);
                    }

                    currentUser.ProfilePicture = guid + ProfilePicture.FileName;
                }
                else {
                    currentUser.ProfilePicture = "defaultuserprofilepicture.png";
                }

                var user = await userManager.GetUserAsync(User);

                // Add action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Update settings",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);

                if (Int32.Parse(currentUser.Gender) == 0)
                {
                    currentUser.Gender = "Female";
                }
                else
                {
                    currentUser.Gender = "Male";
                }

                userRepository.UpdateUser(user.Id ,currentUser);
                return Redirect("/Index");
            }
            catch
            {
                return View();
            }
        }

        // show user information and articles
        // /User/Profile/{UserId}
        public ActionResult Profile(string? Id)
        {
            ViewBag.UserArticles = (from a in dbContext.Articles where a.UserId == Id select a).ToList();
            ViewBag.FollowersCount = dbContext.Follows.Where(f => f.FolloweeId == Id).ToList().Count;
            ViewBag.ArticlesCount = (from a in dbContext.Articles where a.UserId == Id select a).ToList().Count;

            var targetUser = dbContext.Users.FirstOrDefault(u => u.Id == Id);

            if (targetUser == null) {
                return NotFound("maybe you try to access information that dosen't exist!");
            }

            return View(targetUser);
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> Follow(string? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // current user

            if (id == null || userId == id)
            {
                return BadRequest("Invalid user id or you cannot follow yourself.");
            }

            var follow = await dbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FolloweeId == id);

            if (follow == null)
            {
                // Follow logic
                follow = new Follow
                {
                    FollowerId = userId,
                    FolloweeId = id
                };

                dbContext.Follows.Add(follow);

                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = $"Follow {id}",
                    UserId = userId,
                    UserFullName = (from u in dbContext.Users where u.Id == userId select u).FirstOrDefault().FullName
                };
                dbContext.Actions.Add(action);
            }
            else
            {
                // Unfollow logic
                dbContext.Follows.Remove(follow);

                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = $"Unfollow {id}",
                    UserId = userId,
                    UserFullName = (from u in dbContext.Users where u.Id == userId select u).FirstOrDefault().FullName
                };
                dbContext.Actions.Add(action);
            }

            await dbContext.SaveChangesAsync();
            return Redirect($"/User/Profile/{id}");
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<ActionResult> Followers()
        {
            var CurrentUser = await userManager.GetUserAsync(User);
            var FollowersIdCollection = dbContext.Follows
                .Where(f => f.FolloweeId == CurrentUser.Id)
                .Select(f => f.FollowerId)
                .ToList();

            List<AppUser> ListofUsers = new List<AppUser>();

            if (FollowersIdCollection.Any())
            {
                ListofUsers = dbContext.Users
                    .Where(u => FollowersIdCollection.Contains(u.Id))
                    .ToList();
            }

            return View(ListofUsers);
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<ActionResult> Activities() {
            var CurrentUser = await userManager.GetUserAsync(User);
            return View(dbContext.Actions.Where(a => a.UserId == CurrentUser.Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<ActionResult> Bookmarks() {
            var currentUser = await userManager.GetUserAsync(User);
            var list = (from b in dbContext.Bookmarks where b.UserId == currentUser.Id select b).ToList();

            var ArticlesIdCollection = dbContext.Bookmarks
                .Where(b => b.UserId == currentUser.Id)
                .Select(b => b.ArticleId)
                .ToList();

            List<Article> ListOfArticles = new List<Article>();

            if (ArticlesIdCollection.Any())
            {
                ListOfArticles = dbContext.Articles
                    .Where(a => ArticlesIdCollection.Contains(a.ArticleId))
                    .ToList();
            }

            return View(ListOfArticles);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ChangePassword(string password, string newPassword)
        {
            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound("user not found");
            }

            if (await userManager.CheckPasswordAsync(currentUser, newPassword) == false)
            {
                return BadRequest("password given is not valid");
            }

            await userManager.ChangePasswordAsync(currentUser, password, newPassword);
            return Redirect("/User/ChangePassword");
        }
    }
}
