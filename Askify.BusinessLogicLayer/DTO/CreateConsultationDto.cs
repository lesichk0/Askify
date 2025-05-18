namespace Askify.BusinessLogicLayer.DTO
{
    public class CreateConsultationDto
    {
        public string? ExpertId { get; set; }
        public bool IsOpenRequest { get; set; }
        public bool IsPublicable { get; set; }
    }

}
