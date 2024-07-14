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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
        public async Task<ActionResult> Activities()
        {
            var CurrentAdmin = await userManager.GetUserAsync(User);
            return View(dbContext.Actions.Where(a => a.UserId == CurrentAdmin.Id).ToList());
        }
        public async Task<ActionResult> DeleteUser(string? Id)
        {
            try
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
                    foreach (var a in listOfUserActions)
                    {
                        dbContext.Actions.Remove(a);
                    }
                }

                dbContext.Users.Remove(targetUser);

                var admin = await userManager.GetUserAsync(User);
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = $"Delete user {targetUser.Email}",
                    UserId = admin.Id,
                    UserFullName = admin.FullName
                };

                dbContext.Actions.Add(action);

                dbContext.SaveChanges();

                return Redirect("/Admin/Users");
            }
            catch {
                return BadRequest("You try to delete user who have already data in database");
            }
        }
        public async Task<ActionResult> CreateUser(AppUser user ){
            await dbContext.Users.AddAsync(user);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var password = "User1234@";
            
            await userManager.ResetPasswordAsync(user, token, password);
            await userManager.AddToRoleAsync(user, "user");
            
            user.CreatedAt = DateTime.Now;
            user.ProfilePicture = "defaultuserprofilepicture.png";
            user.UserName = user.Email;
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.UserName.ToUpper();

            var admin = await userManager.GetUserAsync(User);

            var action = new Models.Action
            {
                ActionTime = DateTime.Now,
                ActionType = $"Create new User {user.Email}",
                UserId = admin.Id,
                UserFullName = admin.FullName
            };

            dbContext.Actions.Add(action);

            await dbContext.SaveChangesAsync();
            return Redirect("/Admin/Users");
        }
        [HttpGet]
        public ActionResult ChangePassword() {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ChangePassword(string password, string newPassword) {
            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return NotFound("user not found");
            }

            if (await userManager.CheckPasswordAsync(currentUser, newPassword) == false) {
                return BadRequest("password given is not valid");
            }

            await userManager.ChangePasswordAsync(currentUser, password, newPassword);
            return Redirect("/Admin/ChangePassword");
        }
    }
}
