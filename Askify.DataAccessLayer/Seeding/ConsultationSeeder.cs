using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class ConsultationSeeder
    {
        private readonly AppDbContext _context;

        public ConsultationSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Consultations.AnyAsync()) return;

            var users = await _context.Users.ToListAsync();
            if (users.Count < 2) return;

            var faker = new Faker("uk");

            var consultations = new List<Consultation>();

            for (int i = 0; i < 6; i++)
            {
                var user = faker.PickRandom(users);
                var expert = faker.PickRandom(users.Where(u => u.IsVerifiedExpert && u.Id != user.Id).ToList());

                var consultation = new Consultation
                {
                    UserId = user.Id,
                    ExpertId = expert.Id,
                    IsFree = i % 2 == 0,
                    IsPaid = i % 2 != 0,
                    IsOpenRequest = i % 3 == 0,
                    IsPublicable = faker.Random.Bool(),
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    AnsweredAt = DateTime.UtcNow.AddDays(-i + 1),
                    CompletedAt = DateTime.UtcNow.AddDays(-i + 2),
                    Status = "Completed"
                };

                consultations.Add(consultation);
            }

            await _context.Consultations.AddRangeAsync(consultations);
            await _context.SaveChangesAsync();
        }
    }
}
