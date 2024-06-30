using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blogging_Platform.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }
        public string? CommentContent { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TotalLikes { get; set; }

        // one comment may be have multiple replies
        public List<Reply>? Replies { get; set; }

        // create user referance
        public string? UserId { get; set; }
        public AppUser? AppUser { get; set; }

        // create article referance
        public int? ArticleId { get; set; }
        public Article? Article { get; set; }
    }
}
