namespace Askify.BusinessLogicLayer.DTO
{
    public class CreateReportDto
    {
        public string? TargetUserId { get; set; }
        public int? PostId { get; set; }
        public int? ConsultationId { get; set; }
        public string Reason { get; set; } = null!;
    }
}
