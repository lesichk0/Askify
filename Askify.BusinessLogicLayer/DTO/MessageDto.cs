namespace Askify.BusinessLogicLayer.DTO
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateTime SentAt { get; set; }
    }

}
