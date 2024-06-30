using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blogging_Platform.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // each category have many articles
        public List<Article>? Articles { get; set; }
    }
}
