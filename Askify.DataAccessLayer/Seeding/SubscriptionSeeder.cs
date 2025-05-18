using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class SubscriptionSeeder
    {
        private readonly AppDbContext _context;

        public SubscriptionSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Subscriptions.AnyAsync()) return;

            var users = await _context.Users.ToListAsync();
            var faker = new Faker("uk");
            var subscriptions = new List<Subscription>();

            for (int i = 0; i < 6; i++)
            {
                var subscriber = faker.PickRandom(users);
                var target = faker.PickRandom(users.Where(u => u.Id != subscriber.Id).ToList());

                if (subscriptions.Any(s => s.SubscriberId == subscriber.Id && s.TargetUserId == target.Id))
                    continue;

                subscriptions.Add(new Subscription
                {
                    SubscriberId = subscriber.Id,
                    TargetUserId = target.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
                });
            }

            await _context.Subscriptions.AddRangeAsync(subscriptions);
            await _context.SaveChangesAsync();
        }
    }
}
