using Blogging_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blogging_Platform.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
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
            var user = await userManager.GetUserAsync(User);
            return View(user);
        }

        // show user information and articles
        public ActionResult Profile() {
            return View();
        }
    }
}
