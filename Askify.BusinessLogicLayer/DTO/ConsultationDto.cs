namespace Askify.BusinessLogicLayer.DTO
{
    public class ConsultationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? ExpertId { get; set; }
        public string? ExpertName { get; set; }
        public string Title { get; set; } = null!; // Added field
        public string Description { get; set; } = null!; // Added field
        public string? Category { get; set; } // ML-classified category
        public bool IsFree { get; set; }
        public bool IsPaid { get; set; }
        public decimal? Price { get; set; } // Expert's requested price
        public bool IsOpenRequest { get; set; }
        public bool IsPublicable { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<MessageDto>? Messages { get; set; }
    }
}
