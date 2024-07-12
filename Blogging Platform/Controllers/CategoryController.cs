using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Blogging_Platform.ViewModels;
using Blogging_Platform.Models;
using Blogging_Platform.Services;
using Blogging_Platform.Repositories;
using Microsoft.AspNetCore.Authorization;
namespace Blogging_Platform.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MyDbContext dbContext;
        private readonly ICategoryManager categoryManager;
        public CategoryController(MyDbContext dbContext, ICategoryManager categoryManager)
        {
            this.dbContext = dbContext;
            this.categoryManager = categoryManager;
        }
        // GET: CategoryController
        public ActionResult CategoriesList()
        {
            return View(categoryManager.GetCategories());
        }

        // GET: CategoryController
        [Route("Category/Articles/{CategoryId}")]
        public async Task<IActionResult> Articles(int CategoryId)
        {
            var targetCategory = await categoryManager.GetCategoryWithArticlesAsync(CategoryId);

            if (targetCategory == null)
            {
                return NotFound();
            }

            var categoryViewModel = new CategoryViewModel
            {
                CategoryId = targetCategory.CategoryId,
                CategoryName = targetCategory.CategoryName,
                Articles = targetCategory.Articles
            };

            return View(categoryViewModel);
        }


        [Authorize(Roles = "admin")]
        public ActionResult Create(string? CategoryName)
        {
            try
            {
                categoryManager.CreateCategory(CategoryName);
                return Redirect("/Admin/Categories");
            }
            catch
            {
                return View();
            }
        }
        // POST: CategoryController/Delete/5
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                categoryManager.DeleteCategory(id);
                return Redirect("/Admin/Categories");
            }
            catch
            {
                return View();
            }
        }
    }
}
