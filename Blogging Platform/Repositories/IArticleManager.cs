using Blogging_Platform.Models;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Blogging_Platform.Repositories
{
    public interface IArticleManager
    {
        public void CreateArticle(Article article);
        public void EditArticle(int id, Article article);
        public void DeleteArticle(int id);
        public List<Article> GetArticles();
        public Article GetArticleById(int id);
        public List<Article> GetUserArticles(string id);
        public List<Article> GetSearchArticles(string query);
        public List<Comment> GetArticleComments(int id);
    }
}
