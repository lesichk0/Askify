namespace Askify.DataAccessLayer.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string AuthorId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int? RelatedConsultationId { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Author { get; set; } = null!;
        public Consultation? RelatedConsultation { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
