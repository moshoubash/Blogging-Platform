using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blogging_Platform.Models
{
    public class Like
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LikeId { get; set; }

        // User can make multiple Likes
        public string? UserId { get; set; }
        public AppUser? AppUser { get; set; }
        
        // referance from Article class
        public int ArticleId { get; set; }
        public Article? Article { get; set; }
    }
}
