namespace Askify.BusinessLogicLayer.DTO
{
    public class ReportDto
    {
        public int Id { get; set; }
        public string ReporterId { get; set; } = null!;
        public string? TargetUserId { get; set; }
        public int? PostId { get; set; }
        public int? ConsultationId { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

}
