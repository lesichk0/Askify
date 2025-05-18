namespace Askify.BusinessLogicLayer.DTO
{
    public class CreateFeedbackDto
    {
        public string ExpertId { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
