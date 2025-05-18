using Microsoft.AspNetCore.Identity;

namespace Askify.DataAccessLayer.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerifiedExpert { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool HasUsedFreeConsultation { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
        public DateTime? BlockedAt { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Feedback> FeedbacksGiven { get; set; } = new List<Feedback>();
        public ICollection<Feedback> FeedbacksReceived { get; set; } = new List<Feedback>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Report> ReportsFiled { get; set; } = new List<Report>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

}
