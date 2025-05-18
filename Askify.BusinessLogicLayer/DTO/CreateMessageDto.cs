namespace Askify.BusinessLogicLayer.DTO
{
    public class CreateMessageDto
    {
        public string ReceiverId { get; set; } = null!;
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
    }
}
