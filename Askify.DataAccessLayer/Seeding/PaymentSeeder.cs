using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class PaymentSeeder
    {
        private readonly AppDbContext _context;

        public PaymentSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Payments.AnyAsync()) return;

            var faker = new Faker("uk");
            var paidConsultations = await _context.Consultations
                .Include(c => c.User)
                .Where(c => c.IsPaid)
                .ToListAsync();

            var payments = new List<Payment>();

            foreach (var consultation in paidConsultations.Take(6)) // тільки 6
            {
                payments.Add(new Payment
                {
                    UserId = consultation.UserId,
                    ConsultationId = consultation.Id,
                    Amount = faker.Random.Decimal(20, 100),
                    Currency = "UAH",
                    Status = "Success",
                    Provider = "FakePay",
                    Reference = faker.Random.AlphaNumeric(10),
                    PaymentDate = DateTime.UtcNow.AddDays(-1)
                });
            }

            await _context.Payments.AddRangeAsync(payments);
            await _context.SaveChangesAsync();
        }
    }
}
