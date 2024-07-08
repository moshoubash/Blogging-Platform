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
        [Authorize]
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
            
            return View();
        }

        // GET: UserController/Settings/
        // configure profile settings
        [Authorize]
        public async Task<ActionResult> Settings()
        {
            ViewBag.Country = new SelectList(dbContext.Countries, "Id", "Name");

            var user = await userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        [Authorize]
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
                    currentUser.ProfilePicture = "f5159dbc-0166-4ef6-838b-98e236a401c140010.jpg";
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
        [Authorize]
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
                    ActionType = "Follow",
                    UserId = userId,
                    UserFullName = (from u in dbContext.Users where u.Id == userId select u).FirstOrDefault().FullName
                };
                dbContext.Actions.Add(action);
            }
            else
            {
                ViewBag.UserAlreadyLike = true;
                // Unfollow logic
                dbContext.Follows.Remove(follow);

                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Unfollow",
                    UserId = userId,
                    UserFullName = (from u in dbContext.Users where u.Id == userId select u).FirstOrDefault().FullName
                };
                dbContext.Actions.Add(action);
            }

            await dbContext.SaveChangesAsync();
            return Redirect($"/User/Profile/{id}");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Followers()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var list = dbContext.Follows.Where(f => f.FolloweeId == currentUser.Id).ToList();

            return View();
        }
    }
}
