namespace Blogging_Platform.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public string? FollowerId { get; set; }
        public string? FolloweeId { get; set; }
    }
}
