namespace Askify.DataAccessLayer.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string AuthorId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Post Post { get; set; } = null!;
        public User Author { get; set; } = null!;
        public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    }
}
