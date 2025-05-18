namespace Askify.BusinessLogicLayer.DTO
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public List<string> Tags { get; set; } = new();
        public string AuthorId { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}
