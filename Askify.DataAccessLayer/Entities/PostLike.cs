namespace Askify.DataAccessLayer.Entities
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Post Post { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
