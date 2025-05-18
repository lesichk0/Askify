namespace Askify.DataAccessLayer.Entities
{
    public class Report
    {
        public int Id { get; set; }
        public string ReporterId { get; set; } = null!;
        public string? TargetUserId { get; set; }
        public int? PostId { get; set; }
        public int? ConsultationId { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = null!; // Pending, Reviewed, etc.
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public User Reporter { get; set; } = null!;
        public User? TargetUser { get; set; }
        public Post? Post { get; set; }
        public Consultation? Consultation { get; set; }
    }
}
