using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class ReportSeeder
    {
        private readonly AppDbContext _context;

        public ReportSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Reports.AnyAsync()) return;

            var faker = new Faker("uk");
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.ToListAsync();
            var consultations = await _context.Consultations.ToListAsync();

            var reports = new List<Report>();

            for (int i = 0; i < 6; i++)
            {
                var reporter = faker.PickRandom(users);
                var targetPost = i % 2 == 0 ? faker.PickRandom(posts) : null;
                var targetConsultation = i % 2 != 0 ? faker.PickRandom(consultations) : null;

                reports.Add(new Report
                {
                    ReporterId = reporter.Id,
                    TargetUserId = null,
                    PostId = targetPost?.Id,
                    ConsultationId = targetConsultation?.Id,
                    Reason = faker.Lorem.Sentence(),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    ReviewedAt = null
                });
            }

            await _context.Reports.AddRangeAsync(reports);
            await _context.SaveChangesAsync();
        }
    }
}
