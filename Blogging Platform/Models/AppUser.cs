using Microsoft.AspNetCore.Identity;

namespace Blogging_Platform.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Profile settings

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }

        public List<Article>? Articles { get; set; }
        public List<Comment>? Comments { get; set; }
        public List<Like>? Likes { get; set; }
        public List<Reply>? Replies { get; set; }
        public List<Action>? Actions { get; set; }
        public List<Bookmark>? Bookmarks { get; set; }
    }
}
