namespace Askify.DataAccessLayer.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int ConsultationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Status { get; set; } = null!; // pending, success, failed
        public string Provider { get; set; } = null!;
        public string? Reference { get; set; }
        public DateTime PaymentDate { get; set; }

        public User User { get; set; } = null!;
        public Consultation Consultation { get; set; } = null!;
    }
}
