namespace Askify.BusinessLogicLayer.DTO
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public string SubscriberId { get; set; } = null!;
        public string TargetUserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}
