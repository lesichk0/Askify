namespace Askify.BusinessLogicLayer.DTO
{
    public class CommentLikeDto
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}
