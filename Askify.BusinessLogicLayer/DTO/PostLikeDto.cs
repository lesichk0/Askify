namespace Askify.BusinessLogicLayer.DTO
{
    public class PostLikeDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
