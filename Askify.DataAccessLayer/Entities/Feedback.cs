namespace Askify.DataAccessLayer.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string ExpertId { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public User Expert { get; set; } = null!;
    }
}
