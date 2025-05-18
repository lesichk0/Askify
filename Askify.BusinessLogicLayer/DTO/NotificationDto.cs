namespace Askify.BusinessLogicLayer.DTO
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public int EntityId { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
