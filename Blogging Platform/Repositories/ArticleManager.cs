using Blogging_Platform.Models;
using Blogging_Platform.Services;
using Microsoft.EntityFrameworkCore;

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

        List<Comment> IArticleManager.GetArticleComments(int id)
        {
            var comments = (from c in dbContext.Comments select c).Include(c => c.Replies).ToList();
            return comments;
        }

        List<Article> IArticleManager.GetArticles()
        {
            return dbContext.Articles.ToList();
        }

        List<Article> IArticleManager.GetSearchArticles(string query)
        {
            return (from a in dbContext.Articles
                    where a.ArticleTitle.Contains(query) || a.ArticleContent.Contains(query)
                    select a).ToList();
        }

        List<Article> IArticleManager.GetUserArticles(string id)
        {
            return (from a in dbContext.Articles
                    where a.UserId == id
                    select a).ToList();
        }
    }
}
