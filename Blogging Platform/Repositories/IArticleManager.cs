using Blogging_Platform.Models;

namespace Blogging_Platform.Repositories
{
    public interface IArticleManager
    {
        public void CreateArticle(Article article);
        public void EditArticle(int id, Article article);
        public void DeleteArticle(int id);
        public List<Article> GetArticles();
        public Article GetArticleById(int id);
    }
}
