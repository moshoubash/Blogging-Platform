using Blogging_Platform.Models;
using Blogging_Platform.Repositories;
using Blogging_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Blogging_Platform.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleManager articleManager;
        private readonly ICategoryManager categoryManager;
        private readonly UserManager<AppUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly MyDbContext dbContext;
        public ArticleController(ICategoryManager categoryManager, MyDbContext dbContext, IArticleManager articleManager, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            this.articleManager = articleManager;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this.dbContext = dbContext;
            this.categoryManager = categoryManager;
        }
        // GET: ArticleController
        [Authorize(Roles = "user")]
        public async Task<ActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            return View(articleManager.GetUserArticles(currentUser.Id));
        }

        // GET: ArticleController/Details/5
        public ActionResult Details(int id)
        {
            var targetArticle = articleManager.GetArticleById(id);
            var targetCategory = categoryManager.GetCategoryById(targetArticle.CategoryId);
            ViewBag.ArticleCategory = targetCategory.CategoryName;
            ViewBag.ArticleComments = articleManager.GetArticleComments(id);

            var article = dbContext.Articles
               .Include(a => a.Likes)
               .Include(a => a.Comments)
               .FirstOrDefault(a => a.ArticleId == id);

            return View(article);
        }

        // GET: ArticleController/Create
        [Authorize(Roles = "user")]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(dbContext.Categories, "CategoryId", "CategoryName");
            return View();
        }
        // POST: ArticleController/Create
        [Authorize(Roles = "user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Article article, IFormFile ArticleThumbnail)
        {
            try
            {
                if (ArticleThumbnail != null) {
                    var guid = Guid.NewGuid();
                    var wwroot = webHostEnvironment.WebRootPath + "/ArticleThumbnails";
                    var fullPath = System.IO.Path.Combine(wwroot, guid + ArticleThumbnail.FileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        ArticleThumbnail.CopyTo(stream);
                    }

                    article.ArticleThumbnail = guid + ArticleThumbnail.FileName;
                }

                article.CreatedAt = DateTime.Now;
                
                var user = await userManager.GetUserAsync(User);
                article.UserId = user.Id;

                article.UserFullName = user.FullName;

                // Add action to actions table
                var action = new Models.Action { 
                    ActionTime = DateTime.Now,
                    ActionType = "Create Article",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);

                articleManager.CreateArticle(article);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ArticleController/Edit/5
        [Authorize(Roles = "user")]
        public ActionResult Edit(int id)
        {
            ViewBag.CategoryId = new SelectList(dbContext.Categories, "CategoryId", "CategoryName");
            return View(articleManager.GetArticleById(id));
        }

        // POST: ArticleController/Edit/5
        [Authorize(Roles = "user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Article article)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);

                // Edit action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Edit Article",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);

                article.EditAt = DateTime.Now;
                articleManager.EditArticle(id, article);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        // POST: ArticleController/Delete/5
        [Authorize(Roles = "user")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);

                // Delete action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Delete Article",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);

                articleManager.DeleteArticle(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            return View(articleManager.GetSearchArticles(query));
        }

        /*[Authorize]
        public async Task<ActionResult> ToggleLike(int ArticleId)
        {
            // Get the current user
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // check article exists
            var targetArticle = await dbContext.Articles.FirstOrDefaultAsync(x => x.ArticleId == ArticleId);
            if (targetArticle == null)
            {
                return NotFound();
            }

            // Check
            var existingLike = await dbContext.Likes.FirstOrDefaultAsync(l => l.ArticleId == ArticleId && l.UserId == user.Id);

            if (existingLike != null)
            {
                // already liked
                dbContext.Likes.Remove(existingLike);
                
                // Delete action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Remove Like",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);

                await dbContext.SaveChangesAsync();
                return Redirect($"/Article/Details/{ArticleId}");
            }
            else
            {
                // User has not liked the article, so we add a new like
                var like = new Like
                {
                    ArticleId = ArticleId,
                    UserId = user.Id
                };
                dbContext.Likes.Add(like);
                
                // add action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Add Like",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);

                await dbContext.SaveChangesAsync();
                return Redirect($"/Article/Details/{ArticleId}");
            }
        }
*/

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> ToggleLike(int ArticleId)
        {
            // Get the current user
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                Response.StatusCode = 401; // Unauthorized
                return Json(new { success = false, message = "Unauthorized" });
            }

            // check article exists
            var targetArticle = await dbContext.Articles.FirstOrDefaultAsync(x => x.ArticleId == ArticleId);
            if (targetArticle == null)
            {
                return Json(new { success = false, message = "Article not found" });
            }

            // Check if the user already liked the article
            var existingLike = await dbContext.Likes.FirstOrDefaultAsync(l => l.ArticleId == ArticleId && l.UserId == user.Id);

            if (existingLike != null)
            {
                // already liked
                dbContext.Likes.Remove(existingLike);

                // Delete action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Remove Like",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);
            }
            else
            {
                // User has not liked the article, so we add a new like
                var like = new Like
                {
                    ArticleId = ArticleId,
                    UserId = user.Id
                };
                dbContext.Likes.Add(like);

                // add action to actions table
                var action = new Models.Action
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Add Like",
                    UserId = user.Id,
                    UserFullName = user.FullName
                };

                dbContext.Actions.Add(action);
            }

            await dbContext.SaveChangesAsync();

            // Return the updated like count
            var likeCount = await dbContext.Likes.CountAsync(l => l.ArticleId == ArticleId);
            return Json(new { success = true, likeCount = likeCount });
        }

        [Authorize]
        public async Task<ActionResult> AddComment(int ArticleId, string CommentContent) {
            var user = await userManager.GetUserAsync(User);
            var userId = user.Id;

            var comment = new Comment {
                CommentContent = CommentContent,
                CreatedAt = DateTime.Now,
                UserId = userId.ToString(),
                ArticleId = ArticleId,
                UserFullName = user.FullName
            };

            dbContext.Comments.Add(comment);
            await dbContext.SaveChangesAsync();

            return Redirect($"/Article/Details/{ArticleId}");
        }

        [Authorize]
        public async Task<ActionResult> AddReply(Reply reply) { 
            var user = await userManager.GetUserAsync(User);
            var userId = user.Id;

            reply.CreatedAt = DateTime.Now;
            reply.UserFullName = user.FullName;
            reply.UserId = userId;

            dbContext.Replies.Add(reply);
            await dbContext.SaveChangesAsync();

            return Redirect($"/Article/Details/{reply.ArticleId}");
        }
    }
}
