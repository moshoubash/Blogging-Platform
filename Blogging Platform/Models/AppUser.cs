using Microsoft.AspNetCore.Identity;

namespace Blogging_Platform.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Article>? Articles { get; set; }
        public List<Comment>? Comments { get; set; }
        public List<Like>? Likes { get; set; }
        public List<Reply>? Replies { get; set; }
    }
}
