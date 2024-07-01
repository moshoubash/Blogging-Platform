using Blogging_Platform.Models;

namespace Blogging_Platform.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<Article>? Articles { get; set; }
    }
}
