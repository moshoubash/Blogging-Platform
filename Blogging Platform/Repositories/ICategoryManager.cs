using Blogging_Platform.Models;

namespace Blogging_Platform.Repositories
{
    public interface ICategoryManager
    {
        public void CreateCategory(Category category);
        public void DeleteCategory(int id);
        public List<Category> GetCategories();
        public Category GetCategoryById(int id);
    }
}
