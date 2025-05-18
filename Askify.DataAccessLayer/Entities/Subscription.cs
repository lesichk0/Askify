namespace Askify.DataAccessLayer.Entities
{
    public class Subscription
    {
        public int Id { get; set; }
        public string SubscriberId { get; set; } = null!;
        public string TargetUserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public User Subscriber { get; set; } = null!;
        public User TargetUser { get; set; } = null!;
    }
}
