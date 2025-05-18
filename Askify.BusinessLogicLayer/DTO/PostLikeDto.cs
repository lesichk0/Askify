namespace Askify.BusinessLogicLayer.DTO
{
    public class PostLikeDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
