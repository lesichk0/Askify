namespace Askify.DataAccessLayer.Entities
{
    public class SavedPost
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
    }
}
