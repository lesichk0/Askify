namespace Askify.BusinessLogicLayer.DTO
{
    public class UpdateConsultationDto
    {
        public string Status { get; set; } = null!;
        public bool? IsPublicable { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
