namespace Askify.DataAccessLayer.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Type { get; set; } = null!; // Like, Comment, Save, Subscription
        public int EntityId { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
