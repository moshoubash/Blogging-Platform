using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Blogging_Platform.Models
{
    public class Action
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActionId { get; set; }
        public string? ActionType { get; set; }
        public string? UserFullName { get; set; }
        
        public string? UserId { get; set; }
        public AppUser? AppUser { get; set; }
        
        public DateTime ActionTime { get; set; }
    }
}
