using Askify.DataAccessLayer.Data.Repositories;
using Askify.DataAccessLayer.Interfaces.Repositories;
using Askify.DataAccessLayer.Interfaces;

namespace Askify.DataAccessLayer.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Posts = new PostRepository(_context);
            Comments = new CommentRepository(_context);
            Messages = new MessageRepository(_context);
            Consultations = new ConsultationRepository(_context);
            Feedbacks = new FeedbackRepository(_context);
            Notifications = new NotificationRepository(_context);
            Payments = new PaymentRepository(_context);
            Reports = new ReportRepository(_context);
            SavedPosts = new SavedPostRepository(_context);
            Subscriptions = new SubscriptionRepository(_context);
            Tags = new TagRepository(_context);
            PostLikes = new PostLikeRepository(_context);
            CommentLikes = new CommentLikeRepository(_context);
            PostTags = new PostTagRepository(_context);
        }

        public IUserRepository Users { get; }
        public IPostRepository Posts { get; }
        public ICommentRepository Comments { get; }
        public IMessageRepository Messages { get; }
        public IConsultationRepository Consultations { get; }
        public IFeedbackRepository Feedbacks { get; }
        public INotificationRepository Notifications { get; }
        public IPaymentRepository Payments { get; }
        public IReportRepository Reports { get; }
        public ISavedPostRepository SavedPosts { get; }
        public ISubscriptionRepository Subscriptions { get; }
        public ITagRepository Tags { get; }
        public IPostLikeRepository PostLikes { get; }
        public ICommentLikeRepository CommentLikes { get; }
        public IPostTagRepository PostTags { get; }

        public async Task<bool> CompleteAsync() => await _context.SaveChangesAsync() > 0;
        public void Dispose() => _context.Dispose();
    }

}
