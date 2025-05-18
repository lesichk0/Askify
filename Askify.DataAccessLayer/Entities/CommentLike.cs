namespace Askify.DataAccessLayer.Entities
{
    public class CommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Comment Comment { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
