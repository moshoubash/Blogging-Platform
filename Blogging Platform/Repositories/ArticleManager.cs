using Blogging_Platform.Models;
using Blogging_Platform.Services;

namespace Blogging_Platform.Repositories
{
    public class ArticleManager : IArticleManager
    {
        private readonly MyDbContext dbContext;
        public ArticleManager(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        void IArticleManager.CreateArticle(Article article)
        {
            dbContext.Add(article);
            dbContext.SaveChanges();
        }

        void IArticleManager.DeleteArticle(int id)
        {
            var targetArticle = (from a in dbContext.Articles
                                 where a.ArticleId == id
                                 select a).FirstOrDefault();
            dbContext.Articles.Remove(targetArticle);
            dbContext.SaveChanges();
        }

        void IArticleManager.EditArticle(int id, Article article)
        {
            var targetArticle = (from a in dbContext.Articles
                                 where a.ArticleId == id
                                 select a).FirstOrDefault();
            targetArticle.ArticleTitle = article.ArticleTitle;
            targetArticle.ArticleContent = article.ArticleContent;
            targetArticle.ArticleThumbnail = article.ArticleThumbnail;

            dbContext.SaveChanges();
        }

        Article IArticleManager.GetArticleById(int id)
        {
            return (from a in dbContext.Articles where a.ArticleId == id select a).FirstOrDefault();
        }

        List<Article> IArticleManager.GetArticles()
        {
            return dbContext.Articles.ToList();
        }

        List<Article> IArticleManager.GetUserArticles(string id)
        {
            return (from a in dbContext.Articles
                    where a.UserId == id
                    select a).ToList();
        }
    }
}
