using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blogging_Platform.Models
{
    public class Bookmark
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookmarkId { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        public int ArticleId { get; set; }
        public Article? Article { get; set; }
    }
}
