using Blogging_Platform.Models;
using Blogging_Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace Blogging_Platform.Repositories
{
    public class CategoryManager : ICategoryManager
    {
        private readonly MyDbContext dbContext;
        public CategoryManager(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        void ICategoryManager.CreateCategory(Category category)
        {
            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
        }

        void ICategoryManager.DeleteCategory(int id)
        {
            var targetCategory = (from c in dbContext.Categories where c.CategoryId == id select c).FirstOrDefault();
            dbContext.Categories.Remove(targetCategory);
            dbContext.SaveChanges();
        }

        List<Category> ICategoryManager.GetCategories()
        {
            return dbContext.Categories.ToList();
        }

        Category ICategoryManager.GetCategoryById(int id)
        {
            var targetCategory = (from c in dbContext.Categories where c.CategoryId == id select c).FirstOrDefault();
            return targetCategory;
        }

        async Task<Category> ICategoryManager.GetCategoryWithArticlesAsync(int CategoryId)
        {
            return await dbContext.Categories
                             .Include(c => c.Articles)
                             .FirstOrDefaultAsync(c => c.CategoryId == CategoryId);
        }
    }
}
