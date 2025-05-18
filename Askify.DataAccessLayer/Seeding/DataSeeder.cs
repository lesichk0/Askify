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
            Console.WriteLine("Starting seeding process...");
            
            try
            {
                // First, seed roles and users as they're prerequisites for other entities
                await _roleSeeder.SeedAsync();
                Console.WriteLine("Roles seeded successfully");
                
                await _userSeeder.SeedAsync();
                Console.WriteLine("Users seeded successfully");
                
                // Verify users were created
                if (!await _context.Users.AnyAsync())
                {
                    Console.WriteLine("ERROR: No users were created. Aborting seeding process.");
                    return;
                }
                
                // Seed tags
                await _tagSeeder.SeedAsync();
                Console.WriteLine("Tags seeded successfully");
                
                // Seed main entities
                // Each wrapped in try-catch to continue even if one fails
                try {
                    await _consultationSeeder.SeedAsync();
                    Console.WriteLine("Consultations seeded successfully");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding consultations: {ex.Message}");
                }
                
                try {
                    await _postSeeder.SeedAsync();
                    Console.WriteLine("Posts seeded successfully");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding posts: {ex.Message}");
                }
                
                // Check if consultations exist before seeding messages
                if (await _context.Consultations.AnyAsync()) {
                    try {
                        await _messageSeeder.SeedAsync();
                        Console.WriteLine("Messages seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error seeding messages: {ex.Message}");
                    }
                }
                else {
                    Console.WriteLine("No consultations found. Skipping message seeding.");
                }
                
                // Check if posts exist before seeding post-related entities
                if (await _context.Posts.AnyAsync()) {
                    try {
                        await _commentSeeder.SeedAsync();
                        Console.WriteLine("Comments seeded successfully");
                        
                        await _postLikeSeeder.SeedAsync();
                        Console.WriteLine("Post likes seeded successfully");
                        
                        await _savedPostSeeder.SeedAsync();
                        Console.WriteLine("Saved posts seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error seeding post-related entities: {ex.Message}");
                    }
                }
                else {
                    Console.WriteLine("No posts found. Skipping post-related entity seeding.");
                }
                
                // Seed remaining entities
                try {
                    await _feedbackSeeder.SeedAsync();
                    Console.WriteLine("Feedback seeded successfully");
                } 
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding feedback: {ex.Message}");
                }
                
                try {
                    await _subscriptionSeeder.SeedAsync();
                    Console.WriteLine("Subscriptions seeded successfully");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding subscriptions: {ex.Message}");
                }
                
                try {
                    await _paymentSeeder.SeedAsync();
                    Console.WriteLine("Payments seeded successfully");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding payments: {ex.Message}");
                }
                
                try {
                    await _reportSeeder.SeedAsync();
                    Console.WriteLine("Reports seeded successfully");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding reports: {ex.Message}");
                }
                
                try {
                    await _notificationSeeder.SeedAsync();
                    Console.WriteLine("Notifications seeded successfully");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error seeding notifications: {ex.Message}");
                }
                
                Console.WriteLine("Seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"A critical error occurred during seeding: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        // Add a new method to force seed missing data even if some tables have data
        public async Task ForceSeedMissingDataAsync()
        {
            Console.WriteLine("Starting force seeding process for missing data...");
            
            try
            {
                // Always check and seed roles first
                if (!await _context.Roles.AnyAsync())
                {
                    await _roleSeeder.SeedAsync();
                    Console.WriteLine("✅ Roles seeded successfully");
                }
                else
                {
                    Console.WriteLine("ℹ️ Roles already exist, skipping");
                }
                
                // Check and seed users if needed
                if (!await _context.Users.AnyAsync())
                {
                    await _userSeeder.SeedAsync();
                    Console.WriteLine("✅ Users seeded successfully");
                }
                else
                {
                    Console.WriteLine("ℹ️ Users already exist, skipping");
                }
                
                // Seed in logical order to prevent FK constraint issues
                
                // 1. Seed tags first (no dependencies)
                if (!await _context.Tags.AnyAsync())
                {
                    try {
                        await _tagSeeder.SeedAsync();
                        await _context.SaveChangesAsync(); // Make sure tags are saved before continuing
                        Console.WriteLine("✅ Tags seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding tags: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Tags already exist, skipping");
                }
                
                // 2. Seed consultations (depends on users)
                if (!await _context.Consultations.AnyAsync() && await _context.Users.AnyAsync())
                {
                    try {
                        await _consultationSeeder.SeedAsync();
                        await _context.SaveChangesAsync(); // Make sure consultations are saved
                        Console.WriteLine("✅ Consultations seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding consultations: {ex.Message}");
                        Console.WriteLine(ex.InnerException?.Message ?? "No inner exception");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Consultations already exist or users missing, skipping");
                }
                
                // 3. Seed posts (depends on users, optionally on consultations)
                if (!await _context.Posts.AnyAsync() && await _context.Users.AnyAsync())
                {
                    try {
                        await _postSeeder.SeedAsync();
                        await _context.SaveChangesAsync(); // Make sure posts are saved
                        Console.WriteLine("✅ Posts seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding posts: {ex.Message}");
                        Console.WriteLine(ex.InnerException?.Message ?? "No inner exception");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Posts already exist or users missing, skipping");
                }
                
                // 4. Seed messages (depends on consultations)
                if (!await _context.Messages.AnyAsync() && await _context.Consultations.AnyAsync())
                {
                    try {
                        // Double check that we have consultations and users before messages
                        if (await _context.Consultations.CountAsync() > 0)
                        {
                            await _messageSeeder.SeedAsync();
                            Console.WriteLine("✅ Messages seeded successfully");
                        }
                        else {
                            Console.WriteLine("⚠️ No consultations available for message seeding");
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding messages: {ex.Message}");
                        Console.WriteLine(ex.InnerException?.Message ?? "No inner exception");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Messages already exist or consultations missing, skipping");
                }
                
                // Seed comments if empty (and posts exist)
                if (!await _context.Comments.AnyAsync() && await _context.Posts.AnyAsync())
                {
                    try {
                        await _commentSeeder.SeedAsync();
                        Console.WriteLine("✅ Comments seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding comments: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Comments already exist or posts missing, skipping");
                }
                
                // Seed post likes if empty
                if (!await _context.PostLikes.AnyAsync() && await _context.Posts.AnyAsync())
                {
                    try {
                        await _postLikeSeeder.SeedAsync();
                        Console.WriteLine("✅ Post likes seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding post likes: {ex.Message}");
                    }
                }
                
                // Seed saved posts if empty
                if (!await _context.SavedPosts.AnyAsync() && await _context.Posts.AnyAsync())
                {
                    try {
                        await _savedPostSeeder.SeedAsync();
                        Console.WriteLine("✅ Saved posts seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding saved posts: {ex.Message}");
                    }
                }
                
                // Seed feedbacks if empty
                if (!await _context.Feedbacks.AnyAsync())
                {
                    try {
                        await _feedbackSeeder.SeedAsync();
                        Console.WriteLine("✅ Feedbacks seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding feedbacks: {ex.Message}");
                    }
                }
                
                // Seed subscriptions if empty
                if (!await _context.Subscriptions.AnyAsync())
                {
                    try {
                        await _subscriptionSeeder.SeedAsync();
                        Console.WriteLine("✅ Subscriptions seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding subscriptions: {ex.Message}");
                    }
                }
                
                // Seed payments if empty
                if (!await _context.Payments.AnyAsync() && await _context.Consultations.AnyAsync())
                {
                    try {
                        await _paymentSeeder.SeedAsync();
                        Console.WriteLine("✅ Payments seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding payments: {ex.Message}");
                    }
                }
                
                // Seed reports if empty
                if (!await _context.Reports.AnyAsync())
                {
                    try {
                        await _reportSeeder.SeedAsync();
                        Console.WriteLine("✅ Reports seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding reports: {ex.Message}");
                    }
                }
                
                // Seed notifications if empty
                if (!await _context.Notifications.AnyAsync())
                {
                    try {
                        await _notificationSeeder.SeedAsync();
                        Console.WriteLine("✅ Notifications seeded successfully");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"❌ Error seeding notifications: {ex.Message}");
                    }
                }
                
                Console.WriteLine("✅ Force seeding completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ A critical error occurred during force seeding: {ex.Message}");
                Console.WriteLine(ex.InnerException?.Message ?? "No inner exception");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
