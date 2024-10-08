﻿using Blogging_Platform.Models;

namespace Blogging_Platform.Repositories
{
    public interface ICategoryManager
    {
        public void CreateCategory(string? CategoryName);
        public void DeleteCategory(int id);
        public List<Category> GetCategories();
        public Category GetCategoryById(int id);
        public Task<Category> GetCategoryWithArticlesAsync(int CategoryId);
    }
}
