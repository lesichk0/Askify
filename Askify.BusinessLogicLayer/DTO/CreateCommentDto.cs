namespace Askify.BusinessLogicLayer.DTO
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; } = null!;
    }
}
