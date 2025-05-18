using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(u => u.ReportsFiled)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.TargetUser)
                .WithMany()
                .HasForeignKey(r => r.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.FeedbacksGiven)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Expert)
                .WithMany(u => u.FeedbacksReceived)
                .HasForeignKey(f => f.ExpertId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.User)
                .WithMany(u => u.Consultations)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Expert)
                .WithMany()
                .HasForeignKey(c => c.ExpertId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId);

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Subscriber)
                .WithMany()
                .HasForeignKey(s => s.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.TargetUser)
                .WithMany()
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentLike>()
                .HasIndex(cl => new { cl.UserId, cl.CommentId })
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.PostLikes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedPost>()
                .HasOne(sp => sp.Post)
                .WithMany(p => p.SavedPosts)
                .HasForeignKey(sp => sp.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
