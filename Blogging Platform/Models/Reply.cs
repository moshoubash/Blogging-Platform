using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blogging_Platform.Models
{
    public class Reply
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReplyId { get; set; }
        public string? ReplyContent { get; set; }
        public DateTime CreatedAt { get; set; }

        // referance for User
        public string? UserFullName { get; set; }
        public string? UserId { get; set; }
        public AppUser? AppUser { get; set; }

        // referance for Comment Id
        public int CommentId { get; set; }
        public Comment? Comment { get; set; }

        // referance for Article Id
        public int ArticleId { get; set; }
        public Article? Article { get; set; }
    }
}
