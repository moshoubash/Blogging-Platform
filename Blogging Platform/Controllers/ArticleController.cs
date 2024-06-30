using Blogging_Platform.Models;
using Blogging_Platform.Repositories;
using Blogging_Platform.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public ActionResult Index()
        {
            return View(articleManager.GetArticles());
        }

        // GET: ArticleController/Details/5
        public ActionResult Details(int id)
        {
            return View(articleManager.GetArticleById(id));
        }

        // GET: ArticleController/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(dbContext.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: ArticleController/Create
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

                articleManager.CreateArticle(article);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ArticleController/Edit/5
        public ActionResult Edit(int id)
        {
            return View(articleManager.GetArticleById(id));
        }

        // POST: ArticleController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Article article)
        {
            try
            {
                articleManager.EditArticle(id, article);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // POST: ArticleController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                articleManager.DeleteArticle(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
