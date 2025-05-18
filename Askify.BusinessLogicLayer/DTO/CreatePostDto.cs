namespace Askify.BusinessLogicLayer.DTO
{
    public class CreatePostDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public List<string> Tags { get; set; } = new();
        public string? CoverImageUrl { get; set; }
        public int? RelatedConsultationId { get; set; }
    }
}
