namespace Askify.WebAPI.Models
{
    public class ConsultationViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? ExpertId { get; set; }
        public string Title { get; set; } = null!; // Added field
        public string Description { get; set; } = null!; // Added field
        public bool IsFree { get; set; }
        public bool IsOpenRequest { get; set; }
        public bool IsPublicable { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}