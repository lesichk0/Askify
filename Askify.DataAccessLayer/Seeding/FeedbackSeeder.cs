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

            // Get completed consultations to create realistic feedbacks
            var completedConsultations = await _context.Consultations
                .Where(c => c.Status.ToLower() == "completed" && c.ExpertId != null)
                .ToListAsync();

            var faker = new Faker("uk");
            var feedbacks = new List<Feedback>();
            var addedPairs = new HashSet<string>(); // Track user-expert pairs to avoid duplicates

            // Create feedbacks only for completed consultations
            foreach (var consultation in completedConsultations.Take(6))
            {
                var pairKey = $"{consultation.UserId}_{consultation.ExpertId}";
                
                // Skip if we already added feedback for this user-expert pair
                if (addedPairs.Contains(pairKey)) continue;
                
                addedPairs.Add(pairKey);
                
                feedbacks.Add(new Feedback
                {
                    UserId = consultation.UserId,
                    ExpertId = consultation.ExpertId!,
                    ConsultationId = consultation.Id,
                    Rating = faker.Random.Int(3, 5),
                    Comment = faker.Lorem.Sentence(),
                    CreatedAt = consultation.CompletedAt ?? DateTime.UtcNow.AddDays(-faker.Random.Int(1, 30))
                });
            }

            // If no completed consultations exist, don't seed any feedbacks
            // This prevents creating unrealistic feedback data
            if (feedbacks.Count > 0)
            {
                await _context.Feedbacks.AddRangeAsync(feedbacks);
                await _context.SaveChangesAsync();
            }
        }
    }

}
