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
using System.Security.AccessControl;

namespace Blogging_Platform.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly MyDbContext dbContext;
        private readonly ICategoryManager categoryManager;
        private readonly UserManager<AppUser> userManager;
        private readonly IUserRepository userRepository;
        private readonly IWebHostEnvironment webHostEnvironment;
        public AdminController(MyDbContext dbContext, IWebHostEnvironment webHostEnvironment, IUserRepository userRepository, ICategoryManager categoryManager, UserManager<AppUser> userManager)
        {
            this.dbContext = dbContext;
            this.categoryManager = categoryManager;
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Users()
        {
            return View(userRepository.GetUsers());
        }
        
        [HttpGet]
        public async Task<ActionResult> Settings()
        {
            ViewBag.Country = new SelectList(dbContext.Countries, "Id", "Name");

            var user = await userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
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
                else
                {
                    currentUser.ProfilePicture = "defaultuserprofilepicture.png";
                }

                var user = await userManager.GetUserAsync(User);

                if (user == null) {
                    return NotFound("user not match");
                }

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

                userRepository.UpdateUser(user.Id, currentUser);
                return Redirect("/Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Categories()
        {
            return View(categoryManager.GetCategories());
        }

        [HttpGet]
        public ActionResult Activities()
        {
            return View();
        }

        public ActionResult DeleteUser(string? Id)
        {
            var targetUser = (from u in dbContext.Users
                              where u.Id == Id
                              select u).FirstOrDefault();

            if (targetUser == null)
            {
                return NotFound("User not found in database");
            }

            var listOfUserActions = dbContext.Actions.Where(a => a.UserId == Id).ToList();

            if (listOfUserActions != null)
            {
                foreach (var action in listOfUserActions)
                {
                    dbContext.Actions.Remove(action);
                }
            }

            dbContext.Users.Remove(targetUser);
            dbContext.SaveChanges();

            return Redirect("/Admin/Users");
        }
    }
}
