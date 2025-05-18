namespace Askify.DataAccessLayer.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ConsultationId { get; set; }
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime SentAt { get; set; }
        public string Status { get; set; } = "Sent";

        public Consultation Consultation { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}
