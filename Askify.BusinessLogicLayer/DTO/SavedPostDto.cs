namespace Askify.BusinessLogicLayer.DTO
{
    public class SavedPostDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string PostTitle { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}
