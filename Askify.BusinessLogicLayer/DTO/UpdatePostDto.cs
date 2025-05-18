namespace Askify.BusinessLogicLayer.DTO
{
    public class UpdatePostDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? Tags { get; set; }
        public string? CoverImageUrl { get; set; }
    }
}
