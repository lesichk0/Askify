using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class FeedbackSeeder
    {
        private readonly AppDbContext _context;

        public FeedbackSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Feedbacks.AnyAsync()) return;

            var users = await _context.Users.ToListAsync();
            var experts = users.Where(u => u.IsVerifiedExpert).ToList();
            if (experts.Count == 0) return;

            var faker = new Faker("uk");
            var feedbacks = new List<Feedback>();

            for (int i = 0; i < 6; i++)
            {
                var expert = faker.PickRandom(experts);
                var user = faker.PickRandom(users.Where(u => u.Id != expert.Id).ToList());

                feedbacks.Add(new Feedback
                {
                    UserId = user.Id,
                    ExpertId = expert.Id,
                    Rating = faker.Random.Int(3, 5),
                    Comment = faker.Lorem.Sentence(),
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            await _context.Feedbacks.AddRangeAsync(feedbacks);
            await _context.SaveChangesAsync();
        }
    }

}
