namespace Askify.DataAccessLayer.Entities
{
    public class Consultation
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? ExpertId { get; set; }
        public string Title { get; set; } = null!; // Added title field
        public string Description { get; set; } = null!; // Added description field
        public bool IsFree { get; set; }
        public bool IsPaid { get; set; }
        public decimal? Price { get; set; } // Expert's requested price
        public bool IsOpenRequest { get; set; }
        public bool IsPublicable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = null!;

        public User User { get; set; } = null!;
        public User? Expert { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public Payment? Payment { get; set; }
        public ICollection<Post> RelatedPosts { get; set; } = new List<Post>();
    }
}
