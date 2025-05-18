using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class NotificationSeeder
    {
        private readonly AppDbContext _context;

        public NotificationSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Notifications.AnyAsync()) return;

            var faker = new Faker("uk");
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.Include(p => p.Author).ToListAsync();
            var subscriptions = await _context.Subscriptions.Include(s => s.TargetUser).ToListAsync();

            var notifications = new List<Notification>();

            foreach (var post in posts.Take(3))
            {
                var randomUser = faker.PickRandom(users.Where(u => u.Id != post.AuthorId).ToList());

                notifications.Add(new Notification
                {
                    UserId = post.AuthorId,
                    Type = "Like",
                    EntityId = post.Id,
                    Message = $"{randomUser.FullName} liked your post",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5)
                });
            }

            foreach (var sub in subscriptions.Take(3))
            {
                notifications.Add(new Notification
                {
                    UserId = sub.TargetUserId,
                    Type = "Subscription",
                    EntityId = sub.Id,
                    Message = $"{sub.SubscriberId} subscribed to you",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-3)
                });
            }

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
