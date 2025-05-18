using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IPostRepository Posts { get; }
        ICommentRepository Comments { get; }
        IMessageRepository Messages { get; }
        IConsultationRepository Consultations { get; }
        IFeedbackRepository Feedbacks { get; }
        INotificationRepository Notifications { get; }
        IPaymentRepository Payments { get; }
        IReportRepository Reports { get; }
        ISavedPostRepository SavedPosts { get; }
        ISubscriptionRepository Subscriptions { get; }
        ITagRepository Tags { get; }
        IPostLikeRepository PostLikes { get; }
        ICommentLikeRepository CommentLikes { get; }
        IPostTagRepository PostTags { get; }

        Task<bool> CompleteAsync();
    }
}
