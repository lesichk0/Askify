using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly RoleSeeder _roleSeeder;
        private readonly UserSeeder _userSeeder;
        private readonly ConsultationSeeder _consultationSeeder;
        private readonly TagSeeder _tagSeeder;
        private readonly PostSeeder _postSeeder;
        private readonly MessageSeeder _messageSeeder;
        private readonly FeedbackSeeder _feedbackSeeder;
        private readonly SubscriptionSeeder _subscriptionSeeder;
        private readonly PaymentSeeder _paymentSeeder;
        private readonly ReportSeeder _reportSeeder;
        private readonly NotificationSeeder _notificationSeeder;
        private readonly CommentSeeder _commentSeeder;
        private readonly CommentLikeSeeder _commentLikeSeeder;
        private readonly PostLikeSeeder _postLikeSeeder;
        private readonly SavedPostSeeder _savedPostSeeder;

        public DataSeeder(
            AppDbContext context,
            RoleSeeder roleSeeder,
            UserSeeder userSeeder,
            ConsultationSeeder consultationSeeder,
            TagSeeder tagSeeder,
            PostSeeder postSeeder,
            MessageSeeder messageSeeder,
            FeedbackSeeder feedbackSeeder,
            SubscriptionSeeder subscriptionSeeder,
            PaymentSeeder paymentSeeder,
            ReportSeeder reportSeeder,
            NotificationSeeder notificationSeeder,
            CommentSeeder commentSeeder,
            CommentLikeSeeder commentLikeSeeder,
            PostLikeSeeder postLikeSeeder,
            SavedPostSeeder savedPostSeeder)
        {
            _context = context;
            _roleSeeder = roleSeeder;
            _userSeeder = userSeeder;
            _consultationSeeder = consultationSeeder;
            _tagSeeder = tagSeeder;
            _postSeeder = postSeeder;
            _messageSeeder = messageSeeder;
            _feedbackSeeder = feedbackSeeder;
            _subscriptionSeeder = subscriptionSeeder;
            _paymentSeeder = paymentSeeder;
            _reportSeeder = reportSeeder;
            _notificationSeeder = notificationSeeder;
            _commentSeeder = commentSeeder;
            _commentLikeSeeder = commentLikeSeeder;
            _postLikeSeeder = postLikeSeeder;
            _savedPostSeeder = savedPostSeeder;
        }

        public async Task SeedAllAsync()
        {
            await _context.Database.MigrateAsync();

            await _roleSeeder.SeedAsync();
            await _userSeeder.SeedAsync();
            await _consultationSeeder.SeedAsync();
            await _tagSeeder.SeedAsync();
            await _postSeeder.SeedAsync();
            await _messageSeeder.SeedAsync();
            await _feedbackSeeder.SeedAsync();
            await _subscriptionSeeder.SeedAsync();
            await _paymentSeeder.SeedAsync();
            await _reportSeeder.SeedAsync();
            await _notificationSeeder.SeedAsync();
            await _commentSeeder.SeedAsync();
            await _commentLikeSeeder.SeedAsync();
            await _postLikeSeeder.SeedAsync();
            await _savedPostSeeder.SeedAsync();
        }
    }
}
