using Blogging_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blogging_Platform.Services
{
    public class MyDbContext : IdentityDbContext<AppUser>
    {
        public MyDbContext(DbContextOptions<MyDbContext> dbContext) : base(dbContext)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            var admin = new IdentityRole("admin");
            admin.NormalizedName = "admin";
            var user = new IdentityRole("user");
            user.NormalizedName = "user";
            
            modelBuilder.Entity<IdentityRole>().HasData(admin, user);

            // one to many

            modelBuilder.Entity<Article>()
                .HasOne(a => a.AppUser)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Article>()
                .HasOne(a => a.Category)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.CategoryId);

            // one to many
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Articles)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId);

            // one-to-many

            modelBuilder.Entity<Comment>()
            .HasOne(c => c.Article)
            .WithMany(a => a.Comments)
            .HasForeignKey(c => c.ArticleId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.AppUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);

            // one-to-many

            modelBuilder.Entity<Reply>()
                .HasOne(a => a.AppUser)
                .WithMany(u => u.Replies)
                .HasForeignKey(a => a.UserId);

            // one-to-many

            modelBuilder.Entity<Like>()
                .HasOne(a => a.AppUser)
                .WithMany(u => u.Likes)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Like>()
                .HasOne(a => a.Article)
                .WithMany(x => x.Likes)
                .HasForeignKey(a => a.ArticleId);

            // one-to-many

            modelBuilder.Entity<Tag>()
                .HasOne(a => a.Article)
                .WithMany(u => u.Tags)
                .HasForeignKey(a => a.ArticleId);
        }

        public DbSet<Article> Articles { get; set;} 
        public DbSet<Category> Categories { get; set;} 
        public DbSet<Comment> Comments { get; set;} 
        public DbSet<Reply> Replies { get; set;} 
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Like> Likes { get; set; }
    }
}
