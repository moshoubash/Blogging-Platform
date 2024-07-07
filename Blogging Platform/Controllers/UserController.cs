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
        public ActionResult Dashboard()
        {
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
            var targetUser = dbContext.Users.FirstOrDefault(u => u.Id == Id);
            return View(targetUser);
        }

    }
}
