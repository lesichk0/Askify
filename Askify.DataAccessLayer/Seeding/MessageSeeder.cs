using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class MessageSeeder
    {
        private readonly AppDbContext _context;

        public MessageSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Messages.AnyAsync()) return;

            var consultations = await _context.Consultations
                .Include(c => c.User)
                .Include(c => c.Expert)
                .Where(c => c.ExpertId != null)
                .ToListAsync();

            var faker = new Faker("uk");
            var messages = new List<Message>();

            foreach (var consultation in consultations)
            {
                for (int i = 0; i < 3; i++) // 3 повідомлення на кожну консультацію
                {
                    bool userIsSender = i % 2 == 0;

                    messages.Add(new Message
                    {
                        ConsultationId = consultation.Id,
                        SenderId = userIsSender ? consultation.UserId : consultation.ExpertId!,
                        ReceiverId = userIsSender ? consultation.ExpertId! : consultation.UserId,
                        Text = faker.Lorem.Sentence(),
                        SentAt = DateTime.UtcNow.AddMinutes(-i * 10),
                        Status = "Sent"
                    });
                }
            }

            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();
        }
    }
}
